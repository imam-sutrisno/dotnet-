# Assignment: Data Access Layer dengan Dapper

## Tujuan Pembelajaran
Setelah menyelesaikan assignment ini, Anda diharapkan dapat:
1. ✅ Menerapkan result mapping dengan baik
2. ✅ Mengintegrasikan arsitektur dengan baik
3. ✅ Menerapkan query execution dengan baik
4. ✅ Menerapkan RAW SQL best practices dengan baik

---

## Persiapan

1. Pastikan project hands-on sudah berjalan dengan baik
2. Baca dokumentasi di [README.md](./README.md)
3. Pahami kode yang sudah ada
4. Siapkan tools untuk testing (Postman/Swagger UI)

---

## Tugas 1: Result Mapping (Bobot: 25%)

### Level 1: Basic Mapping
**Tugas:**
Tambahkan entity baru `Category` dan implementasikan repository-nya.

**Requirement:**
1. Buat entity `Category.cs` di folder `Domain/Entities/`:
   ```csharp
   public class Category
   {
       public int CategoryId { get; set; }
       public string Name { get; set; }
       public string Description { get; set; }
       public DateTime CreatedAt { get; set; }
   }
   ```

2. Buat interface `ICategoryRepository.cs` di `Domain/Repositories/`:
   ```csharp
   public interface ICategoryRepository
   {
       Task<Category?> GetByIdAsync(int id);
       Task<IEnumerable<Category>> GetAllAsync();
       Task<Category> CreateAsync(Category category);
       Task<bool> UpdateAsync(Category category);
       Task<bool> DeleteAsync(int id);
   }
   ```

3. Implementasikan `CategoryRepository.cs` di `Infrastructure/DataAccess/Repositories/`
   - Gunakan Dapper untuk semua operasi
   - Terapkan proper result mapping
   - Gunakan async/await

4. Tambahkan tabel `Categories` di `DatabaseInitializer.cs`

### Level 2: Multi-Mapping
**Tugas:**
Modifikasi `Product` entity untuk include relasi dengan `Category`, dan implementasikan method untuk mendapatkan product dengan category details.

**Requirement:**
1. Tambahkan property `Category` di `Product` entity:
   ```csharp
   public class Product
   {
       // existing properties...
       public int CategoryId { get; set; }
       public Category? Category { get; set; }
   }
   ```

2. Tambahkan method baru di `IProductRepository`:
   ```csharp
   Task<Product?> GetByIdWithCategoryAsync(int id);
   Task<IEnumerable<Product>> GetAllWithCategoryAsync();
   ```

3. Implementasikan method tersebut menggunakan Dapper multi-mapping
   - Gunakan `QueryAsync<Product, Category, Product>`
   - Terapkan `splitOn` dengan benar

**Kriteria Penilaian:**
- ✅ Mapping berfungsi dengan benar (40%)
- ✅ Tidak ada N+1 query problem (30%)
- ✅ Code readable dan maintainable (30%)

---

## Tugas 2: Integrasi Arsitektur (Bobot: 25%)

### Tugas:
Implementasikan **Service Layer** sebagai tambahan layer antara Controller dan Repository.

**Requirement:**

1. Buat folder `Application/Services/` 

2. Buat interface `IProductService.cs`:
   ```csharp
   public interface IProductService
   {
       Task<ProductResponseDto?> GetProductByIdAsync(int id);
       Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync();
       Task<ProductResponseDto> CreateProductAsync(CreateProductDto dto);
       Task<bool> UpdateProductAsync(int id, UpdateProductDto dto);
       Task<bool> DeleteProductAsync(int id);
       Task<bool> UpdateStockAsync(int productId, int quantity);
   }
   ```

3. Implementasikan `ProductService.cs`:
   - Inject `IProductRepository` via constructor
   - Tambahkan business logic (validation, calculation, etc.)
   - Handle mapping dari Entity ke DTO
   - Implementasikan error handling

4. Buat custom exception `ProductNotFoundException.cs` dan `InsufficientStockException.cs`

5. Update `ProductsController` untuk menggunakan `IProductService` instead of repository directly

6. Register service di `Program.cs` dengan DI

**Contoh Business Logic yang harus diimplementasikan:**
- Validasi: Price harus > 0
- Validasi: Stock tidak boleh negatif
- Business rule: Saat create product, set default stock = 0 jika tidak di-provide
- Business rule: UpdateStock harus check apakah stock mencukupi

**Kriteria Penilaian:**
- ✅ Service layer terpisah dengan baik dari controller dan repository (30%)
- ✅ Business logic diimplementasikan di service layer (30%)
- ✅ Dependency Injection configured dengan benar (20%)
- ✅ Error handling dan validation (20%)

---

## Tugas 3: Query Execution (Bobot: 25%)

### Level 1: Complex Queries
**Tugas:**
Implementasikan fitur reporting dengan query yang complex.

**Requirement:**

1. Tambahkan interface `IReportRepository.cs`:
   ```csharp
   public interface IReportRepository
   {
       Task<IEnumerable<ProductSalesReport>> GetProductSalesReportAsync(DateTime startDate, DateTime endDate);
       Task<IEnumerable<CustomerOrderReport>> GetCustomerOrderReportAsync(int? customerId = null);
       Task<Dictionary<string, decimal>> GetSalesByCategoryAsync(DateTime startDate, DateTime endDate);
       Task<IEnumerable<TopSellingProduct>> GetTopSellingProductsAsync(int topN = 10);
   }
   ```

2. Buat DTO models untuk report results:
   ```csharp
   public class ProductSalesReport
   {
       public int ProductId { get; set; }
       public string ProductName { get; set; }
       public int TotalQuantitySold { get; set; }
       public decimal TotalRevenue { get; set; }
       public int OrderCount { get; set; }
   }
   
   public class CustomerOrderReport
   {
       public int CustomerId { get; set; }
       public string CustomerName { get; set; }
       public int TotalOrders { get; set; }
       public decimal TotalSpent { get; set; }
       public DateTime LastOrderDate { get; set; }
   }
   
   public class TopSellingProduct
   {
       public int ProductId { get; set; }
       public string ProductName { get; set; }
       public int TotalSold { get; set; }
   }
   ```

3. Implementasikan `ReportRepository.cs` dengan SQL queries:
   - Gunakan JOIN, GROUP BY, ORDER BY
   - Gunakan aggregate functions (SUM, COUNT, AVG)
   - Filter by date range
   - Limit results dengan TOP atau LIMIT

4. Buat `ReportsController.cs` untuk expose report endpoints

### Level 2: Stored Procedures
**Tugas:**
Buat stored procedure untuk operasi yang complex dan gunakan Dapper untuk execute.

**Requirement:**

1. Buat stored procedure `sp_ProcessOrder`:
   ```sql
   CREATE PROCEDURE sp_ProcessOrder
       @OrderId INT,
       @NewStatus NVARCHAR(50)
   AS
   BEGIN
       -- Update order status
       -- Update product stock jika order completed
       -- Log order history
       -- Return result
   END
   ```

2. Implementasikan method di repository untuk call stored procedure:
   ```csharp
   public async Task<bool> ProcessOrderAsync(int orderId, string newStatus)
   {
       using var connection = new SqlConnection(_connectionString);
       
       var parameters = new DynamicParameters();
       parameters.Add("@OrderId", orderId);
       parameters.Add("@NewStatus", newStatus);
       parameters.Add("@Success", dbType: DbType.Boolean, direction: ParameterDirection.Output);
       
       await connection.ExecuteAsync(
           "sp_ProcessOrder",
           parameters,
           commandType: CommandType.StoredProcedure
       );
       
       return parameters.Get<bool>("@Success");
   }
   ```

**Kriteria Penilaian:**
- ✅ Complex queries berjalan dengan efisien (30%)
- ✅ Proper use of aggregation dan joins (25%)
- ✅ Stored procedure implementation (25%)
- ✅ Parameterized queries (20%)

---

## Tugas 4: RAW SQL Best Practices (Bobot: 25%)

### Level 1: Security
**Tugas:**
Audit dan improve keamanan query yang ada.

**Requirement:**

1. **Review semua queries** di repository dan pastikan:
   - ✅ Semua input user di-parameterize
   - ✅ Tidak ada string concatenation untuk SQL
   - ✅ Validasi input sebelum query

2. **Implement SQL Injection Test:**
   - Buat unit test yang mencoba SQL injection
   - Pastikan aplikasi aman dari SQL injection

3. **Add Input Validation:**
   - Validate email format di CustomerRepository
   - Validate price range di ProductRepository
   - Validate date ranges di reports

### Level 2: Performance
**Tugas:**
Optimasi query untuk performance.

**Requirement:**

1. **Add Indexes** di DatabaseInitializer:
   ```sql
   CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
   CREATE INDEX IX_Products_Price ON Products(Price);
   CREATE INDEX IX_Orders_OrderDate_Status ON Orders(OrderDate, Status);
   ```

2. **Optimize Queries:**
   - Gunakan SELECT dengan kolom spesifik (bukan SELECT *)
   - Gunakan WHERE clause yang index-friendly
   - Avoid N+1 queries dengan proper joins
   - Use pagination untuk large datasets

3. **Implement Pagination:**
   ```csharp
   public async Task<PagedResult<Product>> GetProductsPagedAsync(
       int pageNumber, 
       int pageSize,
       string? category = null,
       decimal? minPrice = null,
       decimal? maxPrice = null)
   {
       // Implementation with OFFSET and FETCH
   }
   ```

4. **Add Query Performance Logging:**
   - Log slow queries (> 1 second)
   - Log query execution time

### Level 3: Best Practices
**Tugas:**
Terapkan best practices dalam pengelolaan database connection dan transaction.

**Requirement:**

1. **Connection Management:**
   - Pastikan semua connection di-dispose dengan `using`
   - Implement connection pooling configuration
   - Add connection retry logic untuk transient failures

2. **Transaction Management:**
   - Implement transaction untuk multi-step operations
   - Proper rollback on error
   - Transaction isolation level yang sesuai

3. **Error Handling:**
   ```csharp
   public async Task<Product> CreateProductAsync(Product product)
   {
       try
       {
           using var connection = new SqlConnection(_connectionString);
           // ... implementation
       }
       catch (SqlException ex) when (ex.Number == 2627) // Unique constraint
       {
           throw new DuplicateProductException("Product already exists", ex);
       }
       catch (SqlException ex)
       {
           _logger.LogError(ex, "Database error creating product");
           throw new DataAccessException("Error creating product", ex);
       }
   }
   ```

**Kriteria Penilaian:**
- ✅ Security: No SQL injection vulnerabilities (30%)
- ✅ Performance: Optimized queries with indexes (30%)
- ✅ Best Practices: Proper connection and transaction management (25%)
- ✅ Error Handling: Comprehensive error handling (15%)

---

## Deliverables

### Yang Harus Diserahkan:

1. **Source Code**
   - Complete project dengan semua tugas terimplementasi
   - Code harus clean, readable, dan well-structured
   - Include comments untuk logic yang complex

2. **Database Scripts**
   - SQL script untuk create tables
   - SQL script untuk stored procedures
   - SQL script untuk indexes
   - SQL script untuk seed data

3. **API Documentation**
   - Update README.md dengan endpoint baru
   - Swagger documentation harus complete
   - Include example requests/responses

4. **Testing Documentation**
   - List test cases yang sudah dilakukan
   - Screenshot hasil testing via Postman/Swagger
   - Performance test results (optional)

5. **Report Document** (PDF/Word)
   - Penjelasan implementasi setiap tugas
   - Screenshot hasil running aplikasi
   - Challenges yang dihadapi dan solusinya
   - Kesimpulan dan learning points

---

## Grading Rubric

### Excellent (90-100)
- ✅ Semua tugas selesai dengan sempurna
- ✅ Code quality sangat baik (clean, maintainable)
- ✅ Best practices diterapkan konsisten
- ✅ Error handling comprehensive
- ✅ Documentation lengkap dan jelas

### Good (75-89)
- ✅ Sebagian besar tugas selesai
- ✅ Code quality baik
- ✅ Best practices diterapkan
- ✅ Basic error handling
- ✅ Documentation memadai

### Satisfactory (60-74)
- ✅ Tugas utama selesai
- ✅ Code berfungsi dengan benar
- ✅ Beberapa best practices diterapkan
- ✅ Minimal error handling
- ✅ Documentation minimal

### Needs Improvement (<60)
- ❌ Banyak tugas tidak selesai
- ❌ Code quality kurang baik
- ❌ Best practices tidak diterapkan
- ❌ No error handling
- ❌ Documentation kurang

---

## Tips untuk Sukses

1. **Pahami Konsep Dulu**
   - Baca dokumentasi Dapper
   - Pahami clean architecture
   - Review best practices

2. **Start Small**
   - Mulai dari tugas yang paling simple
   - Test setiap implementasi
   - Commit frequently

3. **Testing**
   - Test setiap endpoint yang dibuat
   - Test edge cases
   - Test error scenarios

4. **Code Quality**
   - Follow naming conventions
   - Keep methods small and focused
   - Add comments untuk code yang complex

5. **Ask Questions**
   - Jangan ragu bertanya jika stuck
   - Diskusikan dengan teman
   - Search di Stack Overflow/documentation

---

## Bonus Challenges (Optional) 🚀

Untuk yang ingin tantangan lebih:

1. **Caching Implementation**
   - Implement in-memory caching dengan IMemoryCache
   - Cache frequently accessed data
   - Implement cache invalidation strategy

2. **Unit Testing**
   - Buat unit tests untuk repositories
   - Buat unit tests untuk services
   - Use Moq untuk mocking dependencies

3. **Logging & Monitoring**
   - Implement Serilog untuk structured logging
   - Log semua database operations
   - Add performance metrics

4. **Advanced Features**
   - Implement full-text search
   - Add filtering & sorting
   - Implement bulk operations optimization

---

## Deadline & Submission

- **Deadline**: [Sesuai dengan instruktur]
- **Submission Format**: ZIP file atau GitHub repository link
- **File Name**: `NamaAnda_DapperAssignment.zip`

---

## Kontak & Support

Jika ada pertanyaan:
- Email: [email instruktur]
- Discussion forum: [link]
- Office hours: [schedule]

---

**Good luck dan selamat mengerjakan! 💪**

**Remember:** 
- Quality > Quantity
- Understand > Copy-paste
- Best practices matter!
