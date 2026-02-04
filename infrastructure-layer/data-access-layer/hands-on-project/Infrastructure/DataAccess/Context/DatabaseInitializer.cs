using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ProductAPI.Infrastructure.DataAccess.Context;

public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // Create Tables
        await CreateTablesAsync(connection);
        
        // Seed initial data
        await SeedDataAsync(connection);
    }

    private async Task CreateTablesAsync(SqlConnection connection)
    {
        // Products Table
        var createProductsTable = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
            BEGIN
                CREATE TABLE Products (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    Name NVARCHAR(200) NOT NULL,
                    Description NVARCHAR(1000),
                    Price DECIMAL(18,2) NOT NULL,
                    Stock INT NOT NULL DEFAULT 0,
                    Category NVARCHAR(100),
                    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    UpdatedAt DATETIME2 NULL
                );
                
                CREATE INDEX IX_Products_Category ON Products(Category);
                CREATE INDEX IX_Products_Name ON Products(Name);
            END";
        
        await connection.ExecuteAsync(createProductsTable);

        // Customers Table
        var createCustomersTable = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
            BEGIN
                CREATE TABLE Customers (
                    CustomerId INT PRIMARY KEY IDENTITY(1,1),
                    FullName NVARCHAR(200) NOT NULL,
                    Email NVARCHAR(200) NOT NULL UNIQUE,
                    Phone NVARCHAR(50),
                    Address NVARCHAR(500),
                    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                );
                
                CREATE INDEX IX_Customers_Email ON Customers(Email);
            END";
        
        await connection.ExecuteAsync(createCustomersTable);

        // Orders Table
        var createOrdersTable = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
            BEGIN
                CREATE TABLE Orders (
                    OrderId INT PRIMARY KEY IDENTITY(1,1),
                    CustomerId INT NOT NULL,
                    OrderDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    TotalAmount DECIMAL(18,2) NOT NULL,
                    Status NVARCHAR(50) NOT NULL,
                    FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
                );
                
                CREATE INDEX IX_Orders_CustomerId ON Orders(CustomerId);
                CREATE INDEX IX_Orders_OrderDate ON Orders(OrderDate);
            END";
        
        await connection.ExecuteAsync(createOrdersTable);

        // OrderItems Table
        var createOrderItemsTable = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderItems')
            BEGIN
                CREATE TABLE OrderItems (
                    OrderItemId INT PRIMARY KEY IDENTITY(1,1),
                    OrderId INT NOT NULL,
                    ProductId INT NOT NULL,
                    Quantity INT NOT NULL,
                    UnitPrice DECIMAL(18,2) NOT NULL,
                    TotalPrice DECIMAL(18,2) NOT NULL,
                    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
                    FOREIGN KEY (ProductId) REFERENCES Products(Id)
                );
                
                CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
                CREATE INDEX IX_OrderItems_ProductId ON OrderItems(ProductId);
            END";
        
        await connection.ExecuteAsync(createOrderItemsTable);
    }

    private async Task SeedDataAsync(SqlConnection connection)
    {
        // Check if data already exists
        var productCount = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Products"
        );

        if (productCount > 0)
            return; // Data already seeded

        // Seed Products
        var seedProducts = @"
            INSERT INTO Products (Name, Description, Price, Stock, Category, CreatedAt)
            VALUES
                ('Laptop Dell XPS 13', 'Laptop premium dengan layar 13 inch', 15000000, 10, 'Electronics', GETUTCDATE()),
                ('Mouse Logitech MX Master', 'Mouse wireless ergonomis', 1200000, 25, 'Electronics', GETUTCDATE()),
                ('Keyboard Mechanical', 'Keyboard gaming RGB', 850000, 15, 'Electronics', GETUTCDATE()),
                ('Monitor LG 27 inch', 'Monitor 4K UHD', 4500000, 8, 'Electronics', GETUTCDATE()),
                ('Headset Sony WH-1000XM4', 'Headset noise cancelling', 4200000, 12, 'Electronics', GETUTCDATE()),
                ('Buku Clean Code', 'Buku programming best practices', 250000, 50, 'Books', GETUTCDATE()),
                ('Buku Design Patterns', 'Buku desain pattern software', 300000, 30, 'Books', GETUTCDATE()),
                ('Mouse Pad Gaming', 'Mouse pad ukuran besar', 150000, 40, 'Accessories', GETUTCDATE()),
                ('USB Hub 7 Port', 'USB hub dengan charging', 350000, 20, 'Accessories', GETUTCDATE()),
                ('Webcam Logitech C920', 'Webcam HD 1080p', 1500000, 15, 'Electronics', GETUTCDATE())";
        
        await connection.ExecuteAsync(seedProducts);

        // Seed Customers
        var seedCustomers = @"
            INSERT INTO Customers (FullName, Email, Phone, Address, CreatedAt)
            VALUES
                ('Ahmad Hidayat', 'ahmad@email.com', '08123456789', 'Jakarta Selatan', GETUTCDATE()),
                ('Siti Nurhaliza', 'siti@email.com', '08234567890', 'Bandung', GETUTCDATE()),
                ('Budi Santoso', 'budi@email.com', '08345678901', 'Surabaya', GETUTCDATE()),
                ('Dewi Lestari', 'dewi@email.com', '08456789012', 'Yogyakarta', GETUTCDATE()),
                ('Eko Prasetyo', 'eko@email.com', '08567890123', 'Semarang', GETUTCDATE())";
        
        await connection.ExecuteAsync(seedCustomers);

        // Seed Orders
        var seedOrders = @"
            INSERT INTO Orders (CustomerId, OrderDate, TotalAmount, Status)
            VALUES
                (1, GETUTCDATE(), 16200000, 'Completed'),
                (2, GETUTCDATE(), 850000, 'Processing'),
                (3, GETUTCDATE(), 4500000, 'Shipped')";
        
        await connection.ExecuteAsync(seedOrders);

        // Seed Order Items
        var seedOrderItems = @"
            INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, TotalPrice)
            VALUES
                (1, 1, 1, 15000000, 15000000),
                (1, 2, 1, 1200000, 1200000),
                (2, 3, 1, 850000, 850000),
                (3, 4, 1, 4500000, 4500000)";
        
        await connection.ExecuteAsync(seedOrderItems);
    }
}
