# Migration Guide: SQL Server to MySQL with .NET 6

## Overview
Dokumen ini menjelaskan migrasi dari SQL Server + .NET 8 ke MySQL + .NET 6 dengan implementasi DbConnectionFactory pattern.

## Perubahan Utama

### 1. Framework Version
- **Sebelum**: .NET 8
- **Sesudah**: .NET 6
- **Alasan**: Sesuai requirement untuk menggunakan .NET 6

### 2. Database
- **Sebelum**: SQL Server (Microsoft.Data.SqlClient)
- **Sesudah**: MySQL (MySqlConnector)
- **Alasan**: Migrasi ke MySQL database

### 3. Architecture Pattern
- **Ditambahkan**: DbConnectionFactory pattern untuk reusable database connections
- **Komponen Baru**:
  - `IDbConnectionFactory` - Interface untuk abstraksi connection factory
  - `MySqlConnectionFactory` - Implementasi untuk MySQL

## Detail Perubahan

### A. Project File (ProductAPI.csproj)

#### Sebelum:
```xml
<TargetFramework>net8.0</TargetFramework>
<PackageReference Include="Microsoft.Data.SqlClient" Version="5.1.5" />
```

#### Sesudah:
```xml
<TargetFramework>net6.0</TargetFramework>
<PackageReference Include="MySqlConnector" Version="2.2.5" />
```

### B. Connection String (appsettings.json)

#### Sebelum (SQL Server):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ProductDB;Trusted_Connection=true;TrustServerCertificate=true"
  }
}
```

#### Sesudah (MySQL):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=ProductDB;User=root;Password=yourpassword;"
  }
}
```

### C. SQL Syntax Changes

#### 1. Auto Increment / Identity
**SQL Server:**
```sql
Id INT PRIMARY KEY IDENTITY(1,1)
```

**MySQL:**
```sql
Id INT PRIMARY KEY AUTO_INCREMENT
```

#### 2. String Data Types
**SQL Server:**
```sql
Name NVARCHAR(200)
```

**MySQL:**
```sql
Name VARCHAR(200)
```

#### 3. Date/Time Types
**SQL Server:**
```sql
CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
```

**MySQL:**
```sql
CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
```

#### 4. Getting Last Inserted ID
**SQL Server:**
```sql
SELECT CAST(SCOPE_IDENTITY() as int)
```

**MySQL:**
```sql
SELECT LAST_INSERT_ID()
```

#### 5. Table Creation
**SQL Server:**
```sql
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products (...)
END
```

**MySQL:**
```sql
CREATE TABLE IF NOT EXISTS Products (...)
```

#### 6. Index Creation
**SQL Server:**
```sql
CREATE TABLE Products (
    ...
);
CREATE INDEX IX_Products_Category ON Products(Category);
```

**MySQL:**
```sql
CREATE TABLE Products (
    ...
    INDEX IX_Products_Category (Category)
);
```

### D. DbConnectionFactory Pattern

#### Interface (IDbConnectionFactory.cs)
```csharp
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
```

#### Implementation (MySqlConnectionFactory.cs)
```csharp
public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}
```

#### Usage in Repositories
**Sebelum (Direct Connection String):**
```csharp
public class ProductRepository
{
    private readonly string _connectionString;
    
    public ProductRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task<Product> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        // ...
    }
}
```

**Sesudah (Connection Factory):**
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
        // ...
    }
}
```

### E. Dependency Injection (Program.cs)

**Sebelum:**
```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddScoped<IProductRepository>(sp => 
    new ProductRepository(connectionString));
```

**Sesudah:**
```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register DbConnectionFactory
builder.Services.AddSingleton<IDbConnectionFactory>(sp => 
    new MySqlConnectionFactory(connectionString));

// Register repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
```

## Dapper Examples dari learndapper.com

File `DapperExamples.cs` berisi contoh-contoh lengkap dari learndapper.com:

### 1. Query Methods
- `Query<T>` - Multiple rows
- `QueryFirst<T>` - First row (throw jika tidak ada)
- `QueryFirstOrDefault<T>` - First row or default
- `QuerySingle<T>` - Exactly one row (throw jika bukan satu)
- `QuerySingleOrDefault<T>` - Single row or default

### 2. Execute Methods
- `Execute` - INSERT, UPDATE, DELETE (return rows affected)
- `ExecuteScalar<T>` - Return single value

### 3. Parameters
- Anonymous parameters: `new { Id = id, Name = name }`
- DynamicParameters: untuk parameter yang complex
- List parameters: untuk IN clause

### 4. Relationships (Multi-Mapping)
- One-to-One mapping dengan `splitOn`
- One-to-Many mapping dengan dictionary lookup

### 5. Transactions
- BeginTransaction, Commit, Rollback
- Multiple operations dalam satu transaction

### 6. Multiple Result Sets
- `QueryMultiple` untuk execute multiple queries
- Read multiple result sets dengan `ReadAsync`

### 7. Stored Procedures
- Execute stored procedures
- Output parameters dengan DynamicParameters

### 8. Bulk Operations
- Bulk insert dengan passing IEnumerable

## Keuntungan DbConnectionFactory Pattern

### 1. Abstraksi
- Repository tidak perlu tahu detail connection string
- Mudah untuk switch database provider

### 2. Testability
- Mudah untuk mock IDbConnectionFactory dalam unit test
- Tidak perlu database sesungguhnya untuk testing

### 3. Configuration
- Connection string management terpusat
- Mudah untuk implement connection pooling

### 4. Flexibility
- Bisa implement multiple connection factories (MySQL, PostgreSQL, SQL Server)
- Runtime database selection jika diperlukan

## Setup MySQL dengan Docker

Untuk development, gunakan MySQL Docker container:

```bash
# Pull MySQL image
docker pull mysql:8.0

# Run MySQL container
docker run --name mysql-productdb \
  -e MYSQL_ROOT_PASSWORD=yourpassword \
  -e MYSQL_DATABASE=ProductDB \
  -p 3306:3306 \
  -d mysql:8.0

# Check if running
docker ps

# View logs
docker logs mysql-productdb

# Connect to MySQL
docker exec -it mysql-productdb mysql -uroot -p

# Stop container
docker stop mysql-productdb

# Start container
docker start mysql-productdb

# Remove container
docker rm mysql-productdb
```

## Connection String Options

### Basic Connection String
```
Server=localhost;Port=3306;Database=ProductDB;User=root;Password=yourpassword;
```

### With SSL
```
Server=localhost;Port=3306;Database=ProductDB;User=root;Password=yourpassword;SslMode=Required;
```

### With Connection Pooling
```
Server=localhost;Port=3306;Database=ProductDB;User=root;Password=yourpassword;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;
```

### With Timeout
```
Server=localhost;Port=3306;Database=ProductDB;User=root;Password=yourpassword;ConnectionTimeout=30;
```

## Testing Checklist

- [x] Project builds successfully
- [ ] Database connection works
- [ ] Tables created successfully
- [ ] Seed data inserted
- [ ] CRUD operations work for Products
- [ ] CRUD operations work for Customers
- [ ] CRUD operations work for Orders
- [ ] Multi-mapping works (Order with details)
- [ ] Transactions work properly
- [ ] API endpoints return correct data
- [ ] Swagger UI accessible

## Common Issues & Solutions

### Issue 1: Connection Refused
**Error:** `Unable to connect to any of the specified MySQL hosts`

**Solution:**
- Check MySQL is running: `docker ps`
- Check port 3306 is not blocked by firewall
- Verify connection string is correct

### Issue 2: Authentication Failed
**Error:** `Access denied for user 'root'@'localhost'`

**Solution:**
- Check username and password in connection string
- Reset MySQL root password if needed

### Issue 3: Database Not Found
**Error:** `Unknown database 'ProductDB'`

**Solution:**
- Database will be created automatically on first run
- Or create manually: `CREATE DATABASE ProductDB;`

### Issue 4: Table Already Exists
**Error:** `Table 'Products' already exists`

**Solution:**
- This shouldn't happen with `CREATE TABLE IF NOT EXISTS`
- Check SQL syntax is correct for MySQL

## Migration Steps Summary

1. ✅ Update TargetFramework to net6.0
2. ✅ Replace Microsoft.Data.SqlClient with MySqlConnector
3. ✅ Create IDbConnectionFactory interface
4. ✅ Create MySqlConnectionFactory implementation
5. ✅ Update DatabaseInitializer untuk MySQL syntax
6. ✅ Update all repositories untuk use DbConnectionFactory
7. ✅ Update connection string untuk MySQL
8. ✅ Update SQL queries dari T-SQL ke MySQL
9. ✅ Add Dapper examples dari learndapper.com
10. ✅ Update README.md
11. ✅ Build project successfully
12. ⏳ Test dengan MySQL database

## Referensi

- [MySqlConnector Documentation](https://mysqlconnector.net/)
- [Learn Dapper](https://www.learndapper.com/)
- [MySQL Docker Image](https://hub.docker.com/_/mysql)
- [Dapper GitHub](https://github.com/DapperLib/Dapper)
- [.NET 6 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-6)

---

**Migration Completed!** 🎉

Semua perubahan telah diimplementasikan dengan sukses. Project sekarang menggunakan:
- .NET 6
- MySQL Database
- Reusable DbConnectionFactory Pattern
- Comprehensive Dapper Examples dari learndapper.com
