# Product API - Hands-On Project

## Deskripsi
Ini adalah project Web API menggunakan .NET 8 dengan Dapper sebagai Data Access Layer. Project ini mendemonstrasikan implementasi best practices dalam penggunaan Dapper untuk CRUD operations, result mapping, dan arsitektur yang baik.

## Teknologi yang Digunakan
- ✅ .NET 8 (Web API)
- ✅ Dapper 2.1.28 (Micro-ORM)
- ✅ Microsoft.Data.SqlClient (Database Provider)
- ✅ SQL Server LocalDB
- ✅ Swagger/OpenAPI (API Documentation)

## Struktur Project

```
ProductAPI/
├── Domain/                      # Domain Layer
│   ├── Entities/               # Entity models
│   │   ├── Product.cs
│   │   ├── Customer.cs
│   │   └── Order.cs
│   └── Repositories/           # Repository interfaces
│       ├── IProductRepository.cs
│       ├── ICustomerRepository.cs
│       └── IOrderRepository.cs
│
├── Infrastructure/             # Infrastructure Layer
│   └── DataAccess/
│       ├── Repositories/       # Repository implementations
│       │   ├── ProductRepository.cs
│       │   ├── CustomerRepository.cs
│       │   └── OrderRepository.cs
│       └── Context/
│           └── DatabaseInitializer.cs
│
├── API/                        # API Layer
│   ├── Controllers/            # API Controllers
│   │   ├── ProductsController.cs
│   │   ├── CustomersController.cs
│   │   └── OrdersController.cs
│   └── DTOs/                   # Data Transfer Objects
│       ├── ProductDtos.cs
│       ├── CustomerDtos.cs
│       └── OrderDtos.cs
│
├── Program.cs                  # Application entry point
├── appsettings.json           # Configuration
└── ProductAPI.csproj          # Project file
```

## Fitur yang Diimplementasikan

### 1. Result Mapping ✅
- **Simple Mapping**: Mapping langsung dari query ke entity
- **Custom Column Mapping**: Mapping dengan alias untuk kolom yang berbeda nama
- **Multi-Mapping (One-to-One)**: Mapping relasi Order dengan Customer
- **Multi-Mapping (One-to-Many)**: Mapping relasi Order dengan OrderItems

### 2. Integrasi Arsitektur ✅
- **Clean Architecture**: Pemisahan Domain, Infrastructure, dan API layer
- **Repository Pattern**: Abstraksi data access dengan interface
- **Dependency Injection**: Loose coupling antar komponen
- **DTOs**: Pemisahan model domain dan API response

### 3. Query Execution ✅
- **Query<T>**: Multiple rows (GetAllAsync)
- **QuerySingleOrDefault<T>**: Single row dengan null handling
- **ExecuteScalar<T>**: Single value (GetTotalCountAsync)
- **Execute**: INSERT, UPDATE, DELETE operations
- **QueryMultiple**: Transaction dengan multiple operations

### 4. RAW SQL Best Practices ✅
- **Parameterized Queries**: Semua query menggunakan parameters untuk mencegah SQL injection
- **Specific Column Selection**: SELECT kolom yang dibutuhkan, bukan SELECT *
- **Index-Friendly Queries**: WHERE clause pada kolom yang ter-index
- **Transaction Management**: Proper transaction handling untuk consistency
- **Connection Management**: Using statement untuk auto-dispose
- **Error Handling**: Try-catch dengan logging

## Cara Menjalankan

### Prerequisites
- .NET 8 SDK
- SQL Server atau SQL Server LocalDB
- Visual Studio Code atau Visual Studio 2022 (optional)

### Langkah-langkah

1. **Clone atau Download Project**
   ```bash
   cd hands-on-project
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Update Connection String** (jika perlu)
   
   Edit `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ProductDB;Trusted_Connection=true;TrustServerCertificate=true"
     }
   }
   ```

4. **Build Project**
   ```bash
   dotnet build
   ```

5. **Run Project**
   ```bash
   dotnet run
   ```

6. **Akses Swagger UI**
   
   Buka browser dan kunjungi:
   ```
   https://localhost:7001
   atau
   http://localhost:5001
   ```

## API Endpoints

### Products
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/category/{category}` - Get products by category
- `GET /api/products/search?term={searchTerm}` - Search products
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product
- `GET /api/products/count` - Get total product count

### Customers
- `GET /api/customers` - Get all customers
- `GET /api/customers/{id}` - Get customer by ID
- `GET /api/customers/email/{email}` - Get customer by email
- `POST /api/customers` - Create new customer
- `PUT /api/customers/{id}` - Update customer
- `DELETE /api/customers/{id}` - Delete customer

### Orders
- `GET /api/orders` - Get all orders
- `GET /api/orders/{id}` - Get order by ID
- `GET /api/orders/{id}/details` - Get order with full details (customer + items)
- `GET /api/orders/customer/{customerId}` - Get orders by customer
- `POST /api/orders` - Create new order
- `PATCH /api/orders/{id}/status` - Update order status

## Contoh Request/Response

### Create Product
**Request:**
```http
POST /api/products
Content-Type: application/json

{
  "name": "Laptop Asus ROG",
  "description": "Gaming laptop dengan spesifikasi tinggi",
  "price": 25000000,
  "stock": 5,
  "category": "Electronics"
}
```

**Response:**
```json
{
  "id": 11,
  "name": "Laptop Asus ROG",
  "description": "Gaming laptop dengan spesifikasi tinggi",
  "price": 25000000,
  "stock": 5,
  "category": "Electronics",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null
}
```

### Create Order
**Request:**
```http
POST /api/orders
Content-Type: application/json

{
  "customerId": 1,
  "items": [
    {
      "productId": 1,
      "quantity": 1,
      "unitPrice": 15000000
    },
    {
      "productId": 2,
      "quantity": 2,
      "unitPrice": 1200000
    }
  ]
}
```

**Response:**
```json
{
  "orderId": 4,
  "customerId": 1,
  "orderDate": "2024-01-15T10:30:00Z",
  "totalAmount": 17400000,
  "status": "Pending",
  "customer": null,
  "items": [
    {
      "orderItemId": 0,
      "productId": 1,
      "productName": "Laptop Dell XPS 13",
      "quantity": 1,
      "unitPrice": 15000000,
      "totalPrice": 15000000
    },
    {
      "orderItemId": 0,
      "productId": 2,
      "productName": "Mouse Logitech MX Master",
      "quantity": 2,
      "unitPrice": 1200000,
      "totalPrice": 2400000
    }
  ]
}
```

## Database Schema

Database akan otomatis dibuat saat pertama kali menjalankan aplikasi. Skema yang dibuat:

### Products Table
```sql
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
```

### Customers Table
```sql
CREATE TABLE Customers (
    CustomerId INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    Phone NVARCHAR(50),
    Address NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
```

### Orders Table
```sql
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    OrderDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
);
```

### OrderItems Table
```sql
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
```

## Learning Points

### 1. Result Mapping dengan Dapper
Lihat implementasi di:
- `OrderRepository.GetByIdWithDetailsAsync()` - Multi-mapping one-to-many
- `ProductRepository.GetByIdAsync()` - Simple mapping
- `CustomerRepository.GetByEmailAsync()` - Custom column mapping

### 2. Arsitektur yang Baik
- Pemisahan concerns dengan layer Domain, Infrastructure, API
- Interface untuk abstraksi (IProductRepository, dll)
- Dependency Injection untuk loose coupling
- DTOs untuk data transfer

### 3. Query Execution
- Async/await untuk semua database operations
- Parameterized queries untuk keamanan
- Proper error handling dengan try-catch
- Transaction management untuk data consistency

### 4. RAW SQL Best Practices
- SELECT specific columns, bukan SELECT *
- Parameterized queries untuk prevent SQL injection
- Using statement untuk proper connection disposal
- Index pada kolom yang sering di-query

## Troubleshooting

### Connection Error
Jika mendapat error koneksi database:
1. Pastikan SQL Server LocalDB terinstall
2. Update connection string di `appsettings.json`
3. Coba ganti dengan SQL Server instance lain

### Port Already in Use
Jika port 5001/7001 sudah digunakan:
1. Edit `Properties/launchSettings.json`
2. Ganti port ke port lain yang available

## Next Steps

Setelah memahami project ini, lanjutkan dengan:
1. Kerjakan assignment di [ASSIGNMENT.md](./ASSIGNMENT.md)
2. Tambahkan fitur pagination
3. Implementasi caching
4. Tambahkan unit testing
5. Implementasi authentication/authorization

## Referensi
- [Dapper Documentation](https://github.com/DapperLib/Dapper)
- [ASP.NET Core Web API](https://docs.microsoft.com/en-us/aspnet/core/web-api/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

**Happy Coding! 🚀**
