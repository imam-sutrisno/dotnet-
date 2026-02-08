# Dapper Quick Start Guide

Panduan cepat menggunakan Dapper berdasarkan [learndapper.com](https://www.learndapper.com/)

## Table of Contents
1. [Query Methods](#query-methods)
2. [Execute Methods](#execute-methods)
3. [Parameters](#parameters)
4. [Relationships & Multi-Mapping](#relationships--multi-mapping)
5. [Transactions](#transactions)
6. [Multiple Result Sets](#multiple-result-sets)
7. [Stored Procedures](#stored-procedures)
8. [Best Practices](#best-practices)

---

## Query Methods

### 1. Query - Multiple Rows
Untuk mendapatkan multiple rows dari database.

```csharp
using var connection = _connectionFactory.CreateConnection();

var products = await connection.QueryAsync<Product>(
    "SELECT Id, Name, Price FROM Products WHERE Category = @Category",
    new { Category = "Electronics" }
);
```

### 2. QueryFirst - First Row (Throw if Empty)
Mendapatkan row pertama, throw exception jika tidak ada data.

```csharp
var product = await connection.QueryFirstAsync<Product>(
    "SELECT * FROM Products WHERE Category = @Category ORDER BY Price DESC",
    new { Category = "Electronics" }
);
```

### 3. QueryFirstOrDefault - First Row or Default
Mendapatkan row pertama, return null jika tidak ada.

```csharp
var product = await connection.QueryFirstOrDefaultAsync<Product>(
    "SELECT * FROM Products WHERE Id = @Id",
    new { Id = 999 }
);
// product bisa null
```

### 4. QuerySingle - Exactly One Row
Mendapatkan exactly one row, throw jika 0 atau > 1.

```csharp
var product = await connection.QuerySingleAsync<Product>(
    "SELECT * FROM Products WHERE Id = @Id",
    new { Id = 1 }
);
```

### 5. QuerySingleOrDefault - Single or Default
Mendapatkan single row or default, throw jika > 1.

```csharp
var product = await connection.QuerySingleOrDefaultAsync<Product>(
    "SELECT * FROM Products WHERE Id = @Id",
    new { Id = 1 }
);
```

**Kapan Menggunakan Apa?**
- `Query` → Multiple rows expected
- `QueryFirst` → Want first row, throw if none
- `QueryFirstOrDefault` → Want first row, null if none
- `QuerySingle` → Expect exactly one, throw if 0 or >1
- `QuerySingleOrDefault` → Expect 0 or 1, throw if >1

---

## Execute Methods

### 1. Execute - INSERT, UPDATE, DELETE
Returns number of rows affected.

```csharp
// INSERT
var sql = "INSERT INTO Products (Name, Price) VALUES (@Name, @Price)";
var rowsAffected = await connection.ExecuteAsync(sql, 
    new { Name = "New Product", Price = 100000 });

// UPDATE
var sql = "UPDATE Products SET Price = @Price WHERE Id = @Id";
var rowsAffected = await connection.ExecuteAsync(sql, 
    new { Id = 1, Price = 150000 });

// DELETE
var sql = "DELETE FROM Products WHERE Id = @Id";
var rowsAffected = await connection.ExecuteAsync(sql, 
    new { Id = 1 });
```

### 2. ExecuteScalar - Single Value
Return single value dari query.

```csharp
var count = await connection.ExecuteScalarAsync<int>(
    "SELECT COUNT(*) FROM Products WHERE Category = @Category",
    new { Category = "Electronics" }
);

var maxPrice = await connection.ExecuteScalarAsync<decimal>(
    "SELECT MAX(Price) FROM Products"
);

var newId = await connection.ExecuteScalarAsync<int>(
    "INSERT INTO Products (Name, Price) VALUES (@Name, @Price); SELECT LAST_INSERT_ID()",
    new { Name = "Product", Price = 100 }
);
```

---

## Parameters

### 1. Anonymous Parameters (Paling Umum)
```csharp
var products = await connection.QueryAsync<Product>(
    "SELECT * FROM Products WHERE Category = @Category AND Price > @MinPrice",
    new { Category = "Electronics", MinPrice = 1000000 }
);
```

### 2. DynamicParameters
Untuk scenarios yang lebih complex.

```csharp
var parameters = new DynamicParameters();
parameters.Add("@Name", "Product Name");
parameters.Add("@Price", 100000);
parameters.Add("@Category", "Electronics");

var id = await connection.ExecuteScalarAsync<int>(
    "INSERT INTO Products (Name, Price, Category) VALUES (@Name, @Price, @Category); SELECT LAST_INSERT_ID()",
    parameters
);
```

### 3. List Parameters (IN Clause)
```csharp
var ids = new List<int> { 1, 2, 3, 4, 5 };

var products = await connection.QueryAsync<Product>(
    "SELECT * FROM Products WHERE Id IN @Ids",
    new { Ids = ids }
);
```

### 4. String Parameters
```csharp
var searchTerm = "Laptop";

var products = await connection.QueryAsync<Product>(
    "SELECT * FROM Products WHERE Name LIKE @SearchTerm",
    new { SearchTerm = $"%{searchTerm}%" }
);
```

---

## Relationships & Multi-Mapping

### 1. One-to-One Relationship

```csharp
var sql = @"
    SELECT 
        o.OrderId, o.CustomerId, o.OrderDate, o.TotalAmount, o.Status,
        c.CustomerId, c.FullName, c.Email, c.Phone
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
    new { OrderId = 1 },
    splitOn: "CustomerId"
);
```

**Penting**: `splitOn` menentukan kolom mana yang memulai object kedua.

### 2. One-to-Many Relationship

```csharp
var sql = @"
    SELECT 
        o.OrderId, o.CustomerId, o.OrderDate, o.TotalAmount,
        oi.OrderItemId, oi.ProductId, oi.Quantity, oi.UnitPrice
    FROM Orders o
    LEFT JOIN OrderItems oi ON o.OrderId = oi.OrderId
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
    new { OrderId = 1 },
    splitOn: "OrderItemId"
);

var orderWithItems = orderDict.Values.FirstOrDefault();
```

---

## Transactions

### Simple Transaction
```csharp
using var connection = _connectionFactory.CreateConnection();
connection.Open();

using var transaction = connection.BeginTransaction();

try
{
    // Operation 1
    await connection.ExecuteAsync(
        "INSERT INTO Products (Name, Price) VALUES (@Name, @Price)",
        new { Name = "Product 1", Price = 100 },
        transaction
    );

    // Operation 2
    await connection.ExecuteAsync(
        "UPDATE Products SET Stock = Stock - 1 WHERE Id = @Id",
        new { Id = 1 },
        transaction
    );

    // Commit if all successful
    transaction.Commit();
}
catch
{
    // Rollback if any error
    transaction.Rollback();
    throw;
}
```

### Transaction Best Practices
1. Always use `try-catch-finally` atau `using`
2. Commit hanya jika semua operations berhasil
3. Rollback jika ada error
4. Keep transactions as short as possible

---

## Multiple Result Sets

Gunakan `QueryMultiple` untuk execute multiple queries dalam satu database roundtrip.

```csharp
var sql = @"
    SELECT * FROM Products WHERE Category = @Category;
    SELECT * FROM Customers WHERE CreatedAt > @Date;
    SELECT COUNT(*) FROM Orders WHERE Status = @Status";

using var multi = await connection.QueryMultipleAsync(
    sql,
    new { 
        Category = "Electronics", 
        Date = DateTime.UtcNow.AddDays(-30),
        Status = "Pending"
    }
);

var products = await multi.ReadAsync<Product>();
var customers = await multi.ReadAsync<Customer>();
var orderCount = await multi.ReadSingleAsync<int>();
```

**Keuntungan:**
- Hanya satu database roundtrip
- Lebih efisien daripada multiple queries
- Tetap type-safe

---

## Stored Procedures

### Execute Stored Procedure
```csharp
var result = await connection.QueryAsync<Product>(
    "GetProductsByCategory",
    new { Category = "Electronics" },
    commandType: CommandType.StoredProcedure
);
```

### With Output Parameters
```csharp
var parameters = new DynamicParameters();
parameters.Add("@ProductId", 1);
parameters.Add("@TotalOrders", dbType: DbType.Int32, direction: ParameterDirection.Output);

await connection.ExecuteAsync(
    "GetProductOrderCount",
    parameters,
    commandType: CommandType.StoredProcedure
);

var totalOrders = parameters.Get<int>("@TotalOrders");
```

---

## Best Practices

### 1. Always Use Parameters
❌ **Bad (SQL Injection Risk):**
```csharp
var sql = $"SELECT * FROM Products WHERE Name = '{productName}'";
var products = await connection.QueryAsync<Product>(sql);
```

✅ **Good:**
```csharp
var sql = "SELECT * FROM Products WHERE Name = @Name";
var products = await connection.QueryAsync<Product>(sql, new { Name = productName });
```

### 2. Use Async Methods
✅ **Good:**
```csharp
var products = await connection.QueryAsync<Product>(sql);
```

### 3. Dispose Connections Properly
✅ **Good:**
```csharp
using var connection = _connectionFactory.CreateConnection();
// connection will be disposed automatically
```

### 4. Select Specific Columns
❌ **Bad:**
```csharp
var sql = "SELECT * FROM Products";
```

✅ **Good:**
```csharp
var sql = "SELECT Id, Name, Price, Stock FROM Products";
```

### 5. Use Proper Method for Expected Results
```csharp
// Multiple rows
var products = await connection.QueryAsync<Product>(sql);

// Single row (throw if none)
var product = await connection.QuerySingleAsync<Product>(sql);

// Single row (null if none)
var product = await connection.QuerySingleOrDefaultAsync<Product>(sql);

// First row from many
var product = await connection.QueryFirstOrDefaultAsync<Product>(sql);

// Single value
var count = await connection.ExecuteScalarAsync<int>(sql);
```

### 6. Keep Transactions Short
```csharp
// ✅ Good - short transaction
using var transaction = connection.BeginTransaction();
await connection.ExecuteAsync(sql1, param1, transaction);
await connection.ExecuteAsync(sql2, param2, transaction);
transaction.Commit();

// ❌ Bad - long transaction with business logic
using var transaction = connection.BeginTransaction();
await connection.ExecuteAsync(sql1, param1, transaction);
PerformComplexCalculation(); // Don't do this!
await connection.ExecuteAsync(sql2, param2, transaction);
transaction.Commit();
```

### 7. Handle Nulls Properly
```csharp
// Use nullable types
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } // Nullable for optional fields
    public decimal Price { get; set; }
}
```

### 8. Use Connection Factory
✅ **Good:**
```csharp
public class ProductRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Product> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Product>(
            "SELECT * FROM Products WHERE Id = @Id",
            new { Id = id }
        );
    }
}
```

---

## Common Scenarios

### Pagination
```csharp
public async Task<IEnumerable<Product>> GetProductsPagedAsync(int page, int pageSize)
{
    var offset = (page - 1) * pageSize;
    
    var sql = @"
        SELECT * FROM Products 
        ORDER BY Id 
        LIMIT @PageSize OFFSET @Offset";
    
    using var connection = _connectionFactory.CreateConnection();
    return await connection.QueryAsync<Product>(
        sql, 
        new { PageSize = pageSize, Offset = offset }
    );
}
```

### Bulk Insert
```csharp
public async Task BulkInsertProductsAsync(IEnumerable<Product> products)
{
    var sql = "INSERT INTO Products (Name, Price, Stock) VALUES (@Name, @Price, @Stock)";
    
    using var connection = _connectionFactory.CreateConnection();
    await connection.ExecuteAsync(sql, products);
}
```

### Search with Multiple Conditions
```csharp
public async Task<IEnumerable<Product>> SearchProductsAsync(
    string? searchTerm, 
    string? category, 
    decimal? minPrice, 
    decimal? maxPrice)
{
    var conditions = new List<string>();
    var parameters = new DynamicParameters();

    if (!string.IsNullOrEmpty(searchTerm))
    {
        conditions.Add("Name LIKE @SearchTerm");
        parameters.Add("SearchTerm", $"%{searchTerm}%");
    }

    if (!string.IsNullOrEmpty(category))
    {
        conditions.Add("Category = @Category");
        parameters.Add("Category", category);
    }

    if (minPrice.HasValue)
    {
        conditions.Add("Price >= @MinPrice");
        parameters.Add("MinPrice", minPrice.Value);
    }

    if (maxPrice.HasValue)
    {
        conditions.Add("Price <= @MaxPrice");
        parameters.Add("MaxPrice", maxPrice.Value);
    }

    var whereClause = conditions.Any() 
        ? "WHERE " + string.Join(" AND ", conditions)
        : "";

    var sql = $"SELECT * FROM Products {whereClause}";

    using var connection = _connectionFactory.CreateConnection();
    return await connection.QueryAsync<Product>(sql, parameters);
}
```

---

## Resources

- [Learn Dapper](https://www.learndapper.com/) - Official tutorial
- [Dapper GitHub](https://github.com/DapperLib/Dapper) - Source code & documentation
- [Dapper Tutorial](https://dapper-tutorial.net/) - Comprehensive tutorial

---

**Happy Coding with Dapper! 🚀**
