using System.Data;
using Dapper;
using ProductAPI.Domain.Entities;
using ProductAPI.Domain.Repositories;
using ProductAPI.Infrastructure.DataAccess.Context;

namespace ProductAPI.Infrastructure.DataAccess.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                Id, 
                Name, 
                Description, 
                Price, 
                Stock, 
                Category, 
                CreatedAt, 
                UpdatedAt
            FROM Products 
            WHERE Id = @Id";
        
        return await connection.QuerySingleOrDefaultAsync<Product>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                Id, 
                Name, 
                Description, 
                Price, 
                Stock, 
                Category, 
                CreatedAt, 
                UpdatedAt
            FROM Products 
            ORDER BY Name";
        
        return await connection.QueryAsync<Product>(sql);
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                Id, 
                Name, 
                Description, 
                Price, 
                Stock, 
                Category, 
                CreatedAt, 
                UpdatedAt
            FROM Products 
            WHERE Category = @Category
            ORDER BY Name";
        
        return await connection.QueryAsync<Product>(sql, new { Category = category });
    }

    public async Task<IEnumerable<Product>> SearchByNameAsync(string searchTerm)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                Id, 
                Name, 
                Description, 
                Price, 
                Stock, 
                Category, 
                CreatedAt, 
                UpdatedAt
            FROM Products 
            WHERE Name LIKE @SearchTerm OR Description LIKE @SearchTerm
            ORDER BY Name";
        
        return await connection.QueryAsync<Product>(
            sql, 
            new { SearchTerm = $"%{searchTerm}%" }
        );
    }

    public async Task<Product> CreateAsync(Product product)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        product.CreatedAt = DateTime.UtcNow;
        
        var sql = @"
            INSERT INTO Products (Name, Description, Price, Stock, Category, CreatedAt)
            VALUES (@Name, @Description, @Price, @Stock, @Category, @CreatedAt);
            
            SELECT LAST_INSERT_ID()";
        
        var id = await connection.ExecuteScalarAsync<int>(sql, product);
        product.Id = id;
        
        return product;
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        product.UpdatedAt = DateTime.UtcNow;
        
        var sql = @"
            UPDATE Products 
            SET 
                Name = @Name, 
                Description = @Description,
                Price = @Price, 
                Stock = @Stock,
                Category = @Category,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";
        
        var affectedRows = await connection.ExecuteAsync(sql, product);
        
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = "DELETE FROM Products WHERE Id = @Id";
        
        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
        
        return affectedRows > 0;
    }

    public async Task<int> GetTotalCountAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = "SELECT COUNT(*) FROM Products";
        
        return await connection.ExecuteScalarAsync<int>(sql);
    }
}
