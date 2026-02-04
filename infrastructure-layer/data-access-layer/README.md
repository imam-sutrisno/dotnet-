# Data Access Layer dengan Dapper

## Daftar Isi
- [Pengenalan](#pengenalan)
- [1. Result Mapping](#1-result-mapping)
- [2. Integrasi Arsitektur](#2-integrasi-arsitektur)
- [3. Query Execution](#3-query-execution)
- [4. RAW SQL Best Practice](#4-raw-sql-best-practice)
- [Hands-On Project](#hands-on-project)

---

## Pengenalan

### Apa itu Data Access Layer?
Data Access Layer (DAL) adalah lapisan dalam arsitektur aplikasi yang bertanggung jawab untuk mengelola komunikasi antara aplikasi dan database. DAL mengabstraksi logika akses data dari business logic, sehingga kode lebih terorganisir dan mudah di-maintain.

### Apa itu Dapper?
Dapper adalah micro-ORM (Object-Relational Mapper) yang dikembangkan oleh Stack Overflow. Dapper memberikan performa tinggi mendekati ADO.NET murni, namun dengan kemudahan mapping object yang lebih baik.

**Keunggulan Dapper:**
- ⚡ Performa tinggi (hampir sama dengan ADO.NET)
- 🎯 Simple dan mudah dipelajari
- 🔧 Fleksibel dengan RAW SQL
- 📦 Lightweight (hanya satu file)
- 🔄 Mapping otomatis dari query result ke object

### Instalasi
```bash
dotnet add package Dapper
dotnet add package Microsoft.Data.SqlClient
# atau untuk PostgreSQL
dotnet add package Npgsql
# atau untuk MySQL
dotnet add package MySql.Data
```

---

## 1. Result Mapping

Result mapping adalah proses memetakan hasil query database ke object C#. Dapper melakukan mapping secara otomatis berdasarkan nama kolom dan property.

### 1.1 Basic Mapping

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Query dengan mapping otomatis
public async Task<Product> GetProductById(int id)
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = "SELECT Id, Name, Price, Stock, CreatedAt FROM Products WHERE Id = @Id";
    
    return await connection.QuerySingleOrDefaultAsync<Product>(sql, new { Id = id });
}
```

### 1.2 Custom Column Mapping

Ketika nama kolom database berbeda dengan property C#:

```csharp
public class Customer
{
    public int CustomerId { get; set; }
    public string FullName { get; set; }
    public string EmailAddress { get; set; }
}

// SQL dengan alias untuk mapping
var sql = @"
    SELECT 
        customer_id as CustomerId,
        full_name as FullName,
        email as EmailAddress
    FROM Customers
    WHERE customer_id = @Id";

var customer = await connection.QuerySingleAsync<Customer>(sql, new { Id = customerId });
```

### 1.3 Multi-Mapping (One-to-One)

Mapping untuk relasi antar tabel:

```csharp
public class Order
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public Customer Customer { get; set; }
}

public async Task<Order> GetOrderWithCustomer(int orderId)
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = @"
        SELECT 
            o.OrderId, o.OrderDate, o.TotalAmount,
            c.CustomerId, c.FullName, c.EmailAddress
        FROM Orders o
        INNER JOIN Customers c ON o.CustomerId = c.CustomerId
        WHERE o.OrderId = @OrderId";
    
    var order = await connection.QueryAsync<Order, Customer, Order>(
        sql,
        (order, customer) =>
        {
            order.Customer = customer;
            return order;
        },
        new { OrderId = orderId },
        splitOn: "CustomerId"
    );
    
    return order.FirstOrDefault();
}
```

### 1.4 Multi-Mapping (One-to-Many)

Mapping untuk relasi one-to-many:

```csharp
public class Order
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int OrderItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public async Task<Order> GetOrderWithItems(int orderId)
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = @"
        SELECT 
            o.OrderId, o.OrderDate,
            oi.OrderItemId, oi.ProductId, p.Name as ProductName, 
            oi.Quantity, oi.Price
        FROM Orders o
        LEFT JOIN OrderItems oi ON o.OrderId = oi.OrderId
        LEFT JOIN Products p ON oi.ProductId = p.ProductId
        WHERE o.OrderId = @OrderId";
    
    var orderDict = new Dictionary<int, Order>();
    
    await connection.QueryAsync<Order, OrderItem, Order>(
        sql,
        (order, orderItem) =>
        {
            if (!orderDict.TryGetValue(order.OrderId, out var currentOrder))
            {
                currentOrder = order;
                orderDict.Add(order.OrderId, currentOrder);
            }
            
            if (orderItem != null)
            {
                currentOrder.Items.Add(orderItem);
            }
            
            return currentOrder;
        },
        new { OrderId = orderId },
        splitOn: "OrderItemId"
    );
    
    return orderDict.Values.FirstOrDefault();
}
```

### 1.5 Dynamic Mapping

Untuk kasus dimana struktur result tidak fixed:

```csharp
public async Task<IEnumerable<dynamic>> GetDynamicData(string tableName)
{
    using var connection = new SqlConnection(_connectionString);
    
    // Hati-hati: validate tableName untuk menghindari SQL injection
    var sql = $"SELECT * FROM {tableName}";
    
    return await connection.QueryAsync(sql);
}
```

---

## 2. Integrasi Arsitektur

### 2.1 Clean Architecture dengan Dapper

**Struktur Folder:**
```
MyProject/
├── MyProject.Domain/          # Entities, Interfaces
├── MyProject.Application/     # Business Logic, DTOs
├── MyProject.Infrastructure/  # Data Access Implementation
│   └── DataAccess/
│       ├── Repositories/
│       └── DbContext/
└── MyProject.API/            # Web API Controllers
```

### 2.2 Repository Pattern

**Interface (Domain Layer):**
```csharp
namespace MyProject.Domain.Repositories
{
    public interface IProductRepository
    {
        Task<Product> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> CreateAsync(Product product);
        Task<bool> UpdateAsync(Product product);
        Task<bool> DeleteAsync(int id);
    }
}
```

**Implementation (Infrastructure Layer):**
```csharp
using Dapper;
using Microsoft.Data.SqlClient;
using MyProject.Domain.Entities;
using MyProject.Domain.Repositories;

namespace MyProject.Infrastructure.DataAccess.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = "SELECT * FROM Products WHERE Id = @Id";
            
            return await connection.QuerySingleOrDefaultAsync<Product>(
                sql, 
                new { Id = id }
            );
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = "SELECT * FROM Products ORDER BY Name";
            
            return await connection.QueryAsync<Product>(sql);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = @"
                INSERT INTO Products (Name, Price, Stock, CreatedAt)
                VALUES (@Name, @Price, @Stock, @CreatedAt);
                
                SELECT CAST(SCOPE_IDENTITY() as int)";
            
            var id = await connection.ExecuteScalarAsync<int>(sql, product);
            product.Id = id;
            
            return product;
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = @"
                UPDATE Products 
                SET Name = @Name, 
                    Price = @Price, 
                    Stock = @Stock
                WHERE Id = @Id";
            
            var affectedRows = await connection.ExecuteAsync(sql, product);
            
            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = "DELETE FROM Products WHERE Id = @Id";
            
            var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
            
            return affectedRows > 0;
        }
    }
}
```

### 2.3 Unit of Work Pattern

**Interface:**
```csharp
namespace MyProject.Domain.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        ICustomerRepository Customers { get; }
        IOrderRepository Orders { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
```

**Implementation:**
```csharp
using System.Data;
using Microsoft.Data.SqlClient;
using MyProject.Domain.Repositories;
using MyProject.Domain.UnitOfWork;

namespace MyProject.Infrastructure.DataAccess.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SqlConnection _connection;
        private SqlTransaction _transaction;
        private bool _disposed;

        public IProductRepository Products { get; }
        public ICustomerRepository Customers { get; }
        public IOrderRepository Orders { get; }

        public UnitOfWork(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
            _connection.Open();

            Products = new ProductRepository(_connection, _transaction);
            Customers = new CustomerRepository(_connection, _transaction);
            Orders = new OrderRepository(_connection, _transaction);
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = _connection.BeginTransaction();
            
            // Update repositories dengan transaction
            ((ProductRepository)Products).SetTransaction(_transaction);
            ((CustomerRepository)Customers).SetTransaction(_transaction);
            ((OrderRepository)Orders).SetTransaction(_transaction);
        }

        public async Task CommitAsync()
        {
            try
            {
                _transaction?.Commit();
            }
            catch
            {
                _transaction?.Rollback();
                throw;
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            _transaction?.Rollback();
            _transaction?.Dispose();
            _transaction = null;
        }

        public Task<int> SaveChangesAsync()
        {
            // Dalam Dapper, save otomatis terjadi saat execute
            return Task.FromResult(0);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _connection?.Dispose();
            }
            _disposed = true;
        }
    }
}
```

### 2.4 Dependency Injection

**Program.cs (.NET 6+):**
```csharp
using MyProject.Domain.Repositories;
using MyProject.Infrastructure.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register Repositories
builder.Services.AddScoped<IProductRepository>(sp => 
    new ProductRepository(connectionString));
builder.Services.AddScoped<ICustomerRepository>(sp => 
    new CustomerRepository(connectionString));

// atau dengan factory pattern
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddSingleton(connectionString); // jika repository butuh di constructor

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## 3. Query Execution

### 3.1 Query Methods

Dapper menyediakan berbagai method untuk eksekusi query:

#### Query<T>
Mengembalikan IEnumerable<T> untuk multiple rows:

```csharp
public async Task<IEnumerable<Product>> GetProducts()
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = "SELECT * FROM Products";
    
    return await connection.QueryAsync<Product>(sql);
}
```

#### QuerySingle<T> & QuerySingleOrDefault<T>
Untuk single row (error jika > 1 row):

```csharp
public async Task<Product> GetProductById(int id)
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = "SELECT * FROM Products WHERE Id = @Id";
    
    // QuerySingle: error jika tidak ada atau > 1 row
    return await connection.QuerySingleAsync<Product>(sql, new { Id = id });
    
    // QuerySingleOrDefault: return null jika tidak ada
    // return await connection.QuerySingleOrDefaultAsync<Product>(sql, new { Id = id });
}
```

#### QueryFirst<T> & QueryFirstOrDefault<T>
Mengambil row pertama:

```csharp
public async Task<Product> GetFirstProduct()
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = "SELECT TOP 1 * FROM Products ORDER BY CreatedAt DESC";
    
    return await connection.QueryFirstOrDefaultAsync<Product>(sql);
}
```

#### Execute
Untuk INSERT, UPDATE, DELETE:

```csharp
public async Task<int> UpdateProductPrice(int id, decimal newPrice)
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = "UPDATE Products SET Price = @Price WHERE Id = @Id";
    
    // Returns number of affected rows
    return await connection.ExecuteAsync(sql, new { Id = id, Price = newPrice });
}
```

#### ExecuteScalar
Untuk mendapatkan single value:

```csharp
public async Task<int> GetProductCount()
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = "SELECT COUNT(*) FROM Products";
    
    return await connection.ExecuteScalarAsync<int>(sql);
}
```

### 3.2 Parameterized Queries

**Anonymous Object Parameters:**
```csharp
var product = await connection.QuerySingleAsync<Product>(
    "SELECT * FROM Products WHERE Id = @Id AND Price > @MinPrice",
    new { Id = 1, MinPrice = 100 }
);
```

**Object Parameters:**
```csharp
var parameters = new Product { Id = 1, Name = "New Product" };
await connection.ExecuteAsync(
    "UPDATE Products SET Name = @Name WHERE Id = @Id",
    parameters
);
```

**DynamicParameters:**
```csharp
var parameters = new DynamicParameters();
parameters.Add("@Id", 1);
parameters.Add("@Name", "Product Name");
parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

await connection.ExecuteAsync(
    "GetProductById",
    parameters,
    commandType: CommandType.StoredProcedure
);

int totalCount = parameters.Get<int>("@TotalCount");
```

### 3.3 Bulk Operations

**Bulk Insert:**
```csharp
public async Task<int> BulkInsertProducts(IEnumerable<Product> products)
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = @"
        INSERT INTO Products (Name, Price, Stock, CreatedAt)
        VALUES (@Name, @Price, @Stock, @CreatedAt)";
    
    return await connection.ExecuteAsync(sql, products);
}
```

**Bulk Update:**
```csharp
public async Task<int> BulkUpdatePrices(IEnumerable<Product> products)
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = "UPDATE Products SET Price = @Price WHERE Id = @Id";
    
    return await connection.ExecuteAsync(sql, products);
}
```

### 3.4 Multiple Result Sets

```csharp
public async Task<(IEnumerable<Product>, IEnumerable<Customer>)> GetProductsAndCustomers()
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = @"
        SELECT * FROM Products;
        SELECT * FROM Customers;";
    
    using var multi = await connection.QueryMultipleAsync(sql);
    
    var products = await multi.ReadAsync<Product>();
    var customers = await multi.ReadAsync<Customer>();
    
    return (products, customers);
}
```

### 3.5 Stored Procedures

```csharp
public async Task<IEnumerable<Product>> GetProductsByCategory(int categoryId)
{
    using var connection = new SqlConnection(_connectionString);
    
    var parameters = new { CategoryId = categoryId };
    
    return await connection.QueryAsync<Product>(
        "sp_GetProductsByCategory",
        parameters,
        commandType: CommandType.StoredProcedure
    );
}
```

---

## 4. RAW SQL Best Practice

### 4.1 SQL Injection Prevention

**❌ JANGAN LAKUKAN INI:**
```csharp
// BAHAYA: SQL Injection vulnerability
public async Task<Product> GetProductByName(string name)
{
    var sql = $"SELECT * FROM Products WHERE Name = '{name}'";
    return await connection.QueryFirstAsync<Product>(sql);
}
```

**✅ LAKUKAN INI:**
```csharp
// AMAN: Gunakan parameters
public async Task<Product> GetProductByName(string name)
{
    var sql = "SELECT * FROM Products WHERE Name = @Name";
    return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { Name = name });
}
```

### 4.2 Query Optimization

**Gunakan SELECT Specific Columns:**
```csharp
// ❌ Hindari SELECT *
var sql = "SELECT * FROM Products";

// ✅ Specify kolom yang dibutuhkan
var sql = "SELECT Id, Name, Price FROM Products";
```

**Gunakan WHERE untuk Filter:**
```csharp
// ✅ Filter di database, bukan di aplikasi
var sql = @"
    SELECT Id, Name, Price 
    FROM Products 
    WHERE Price > @MinPrice 
    AND Stock > 0";
```

**Gunakan INDEX dengan baik:**
```csharp
// Pastikan kolom di WHERE clause memiliki index
var sql = @"
    SELECT * FROM Orders 
    WHERE CustomerId = @CustomerId  -- Index on CustomerId
    AND OrderDate >= @StartDate";    -- Index on OrderDate
```

### 4.3 Transaction Management

**Simple Transaction:**
```csharp
public async Task<bool> TransferStock(int fromProductId, int toProductId, int quantity)
{
    using var connection = new SqlConnection(_connectionString);
    connection.Open();
    
    using var transaction = connection.BeginTransaction();
    
    try
    {
        // Kurangi stock produk pertama
        var sqlDeduct = @"
            UPDATE Products 
            SET Stock = Stock - @Quantity 
            WHERE Id = @Id AND Stock >= @Quantity";
            
        var deducted = await connection.ExecuteAsync(
            sqlDeduct, 
            new { Id = fromProductId, Quantity = quantity },
            transaction
        );
        
        if (deducted == 0)
            throw new Exception("Insufficient stock");
        
        // Tambah stock produk kedua
        var sqlAdd = @"
            UPDATE Products 
            SET Stock = Stock + @Quantity 
            WHERE Id = @Id";
            
        await connection.ExecuteAsync(
            sqlAdd,
            new { Id = toProductId, Quantity = quantity },
            transaction
        );
        
        transaction.Commit();
        return true;
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}
```

### 4.4 Connection Management

**✅ BEST PRACTICE: Using Statement**
```csharp
public async Task<Product> GetProduct(int id)
{
    using var connection = new SqlConnection(_connectionString);
    // Connection otomatis dibuka oleh Dapper
    // Connection otomatis ditutup setelah using block
    
    var sql = "SELECT * FROM Products WHERE Id = @Id";
    return await connection.QuerySingleAsync<Product>(sql, new { Id = id });
}
```

**❌ HINDARI: Manual Connection Management**
```csharp
// Risiko connection leak jika terjadi exception
public async Task<Product> GetProduct(int id)
{
    var connection = new SqlConnection(_connectionString);
    connection.Open();
    
    var sql = "SELECT * FROM Products WHERE Id = @Id";
    var product = await connection.QuerySingleAsync<Product>(sql, new { Id = id });
    
    connection.Close(); // Might not be called if exception occurs
    return product;
}
```

### 4.5 Error Handling

```csharp
public async Task<Product> GetProductSafe(int id)
{
    try
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = "SELECT * FROM Products WHERE Id = @Id";
        
        return await connection.QuerySingleOrDefaultAsync<Product>(sql, new { Id = id });
    }
    catch (SqlException ex)
    {
        // Log error
        _logger.LogError(ex, "Database error while getting product {ProductId}", id);
        throw new DataAccessException("Error accessing product data", ex);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error while getting product {ProductId}", id);
        throw;
    }
}
```

### 4.6 Async/Await Best Practice

```csharp
// ✅ Gunakan async/await
public async Task<IEnumerable<Product>> GetProductsAsync()
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = "SELECT * FROM Products";
    
    return await connection.QueryAsync<Product>(sql);
}

// ❌ Jangan block async dengan .Result atau .Wait()
public IEnumerable<Product> GetProducts()
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = "SELECT * FROM Products";
    
    // HINDARI: Blocking async call
    return connection.QueryAsync<Product>(sql).Result;
}
```

### 4.7 Performance Tips

**1. Reuse Connection String:**
```csharp
// ✅ Inject connection string, bukan create setiap kali
public class ProductRepository
{
    private readonly string _connectionString;
    
    public ProductRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
}
```

**2. Use Buffered Queries (Default):**
```csharp
// Default: buffered = true (load semua ke memory)
var products = await connection.QueryAsync<Product>(sql);

// Untuk dataset besar, gunakan buffered = false (streaming)
var products = await connection.QueryAsync<Product>(sql, buffered: false);
```

**3. Use CommandTimeout untuk long-running queries:**
```csharp
var sql = "EXEC sp_LongRunningProcedure";

var result = await connection.QueryAsync<Result>(
    sql,
    commandTimeout: 300  // 5 minutes
);
```

---

## Hands-On Project

Lihat folder [hands-on-project](./hands-on-project/) untuk project lengkap Web API menggunakan Dapper.

**Project meliputi:**
- ✅ .NET Core 8 Web API
- ✅ Dapper untuk Data Access
- ✅ Repository Pattern
- ✅ CRUD Operations
- ✅ Result Mapping Examples
- ✅ Transaction Management
- ✅ Error Handling
- ✅ Best Practices Implementation

**Cara menjalankan project:**
```bash
cd hands-on-project
dotnet restore
dotnet build
dotnet run
```

Akses API di: `https://localhost:7001/swagger`

---

## Referensi

- [Dapper Official Documentation](https://github.com/DapperLib/Dapper)
- [Dapper Tutorial](https://www.learndapper.com/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)

---

## Latihan/Tugas

Kerjakan latihan pada folder [hands-on-project](./hands-on-project/ASSIGNMENT.md) untuk menguji pemahaman Anda tentang:
1. Result Mapping dengan berbagai skenario
2. Implementasi arsitektur yang baik
3. Query execution yang efisien
4. RAW SQL best practices

**Selamat belajar! 🚀**
