using System.Data;
using Dapper;

namespace ProductAPI.Infrastructure.DataAccess.Context;

public class DatabaseInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DatabaseInitializer(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        // Create Tables
        await CreateTablesAsync(connection);
        
        // Seed initial data
        await SeedDataAsync(connection);
    }

    private async Task CreateTablesAsync(IDbConnection connection)
    {
        // Products Table
        var createProductsTable = @"
            CREATE TABLE IF NOT EXISTS Products (
                Id INT PRIMARY KEY AUTO_INCREMENT,
                Name VARCHAR(200) NOT NULL,
                Description VARCHAR(1000),
                Price DECIMAL(18,2) NOT NULL,
                Stock INT NOT NULL DEFAULT 0,
                Category VARCHAR(100),
                CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt DATETIME NULL,
                INDEX IX_Products_Category (Category),
                INDEX IX_Products_Name (Name)
            )";
        
        await connection.ExecuteAsync(createProductsTable);

        // Customers Table
        var createCustomersTable = @"
            CREATE TABLE IF NOT EXISTS Customers (
                CustomerId INT PRIMARY KEY AUTO_INCREMENT,
                FullName VARCHAR(200) NOT NULL,
                Email VARCHAR(200) NOT NULL UNIQUE,
                Phone VARCHAR(50),
                Address VARCHAR(500),
                CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                INDEX IX_Customers_Email (Email)
            )";
        
        await connection.ExecuteAsync(createCustomersTable);

        // Orders Table
        var createOrdersTable = @"
            CREATE TABLE IF NOT EXISTS Orders (
                OrderId INT PRIMARY KEY AUTO_INCREMENT,
                CustomerId INT NOT NULL,
                OrderDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                TotalAmount DECIMAL(18,2) NOT NULL,
                Status VARCHAR(50) NOT NULL,
                INDEX IX_Orders_CustomerId (CustomerId),
                INDEX IX_Orders_OrderDate (OrderDate),
                FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
            )";
        
        await connection.ExecuteAsync(createOrdersTable);

        // OrderItems Table
        var createOrderItemsTable = @"
            CREATE TABLE IF NOT EXISTS OrderItems (
                OrderItemId INT PRIMARY KEY AUTO_INCREMENT,
                OrderId INT NOT NULL,
                ProductId INT NOT NULL,
                Quantity INT NOT NULL,
                UnitPrice DECIMAL(18,2) NOT NULL,
                TotalPrice DECIMAL(18,2) NOT NULL,
                INDEX IX_OrderItems_OrderId (OrderId),
                INDEX IX_OrderItems_ProductId (ProductId),
                FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
                FOREIGN KEY (ProductId) REFERENCES Products(Id)
            )";
        
        await connection.ExecuteAsync(createOrderItemsTable);
    }

    private async Task SeedDataAsync(IDbConnection connection)
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
                ('Laptop Dell XPS 13', 'Laptop premium dengan layar 13 inch', 15000000, 10, 'Electronics', UTC_TIMESTAMP()),
                ('Mouse Logitech MX Master', 'Mouse wireless ergonomis', 1200000, 25, 'Electronics', UTC_TIMESTAMP()),
                ('Keyboard Mechanical', 'Keyboard gaming RGB', 850000, 15, 'Electronics', UTC_TIMESTAMP()),
                ('Monitor LG 27 inch', 'Monitor 4K UHD', 4500000, 8, 'Electronics', UTC_TIMESTAMP()),
                ('Headset Sony WH-1000XM4', 'Headset noise cancelling', 4200000, 12, 'Electronics', UTC_TIMESTAMP()),
                ('Buku Clean Code', 'Buku programming best practices', 250000, 50, 'Books', UTC_TIMESTAMP()),
                ('Buku Design Patterns', 'Buku desain pattern software', 300000, 30, 'Books', UTC_TIMESTAMP()),
                ('Mouse Pad Gaming', 'Mouse pad ukuran besar', 150000, 40, 'Accessories', UTC_TIMESTAMP()),
                ('USB Hub 7 Port', 'USB hub dengan charging', 350000, 20, 'Accessories', UTC_TIMESTAMP()),
                ('Webcam Logitech C920', 'Webcam HD 1080p', 1500000, 15, 'Electronics', UTC_TIMESTAMP())";
        
        await connection.ExecuteAsync(seedProducts);

        // Seed Customers
        var seedCustomers = @"
            INSERT INTO Customers (FullName, Email, Phone, Address, CreatedAt)
            VALUES
                ('Ahmad Hidayat', 'ahmad@email.com', '08123456789', 'Jakarta Selatan', UTC_TIMESTAMP()),
                ('Siti Nurhaliza', 'siti@email.com', '08234567890', 'Bandung', UTC_TIMESTAMP()),
                ('Budi Santoso', 'budi@email.com', '08345678901', 'Surabaya', UTC_TIMESTAMP()),
                ('Dewi Lestari', 'dewi@email.com', '08456789012', 'Yogyakarta', UTC_TIMESTAMP()),
                ('Eko Prasetyo', 'eko@email.com', '08567890123', 'Semarang', UTC_TIMESTAMP())";
        
        await connection.ExecuteAsync(seedCustomers);

        // Seed Orders
        var seedOrders = @"
            INSERT INTO Orders (CustomerId, OrderDate, TotalAmount, Status)
            VALUES
                (1, UTC_TIMESTAMP(), 16200000, 'Completed'),
                (2, UTC_TIMESTAMP(), 850000, 'Processing'),
                (3, UTC_TIMESTAMP(), 4500000, 'Shipped')";
        
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
