using Dapper;
using Microsoft.Data.SqlClient;
using ProductAPI.Domain.Entities;
using ProductAPI.Domain.Repositories;

namespace ProductAPI.Infrastructure.DataAccess.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly string _connectionString;

    public CustomerRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            SELECT 
                CustomerId, 
                FullName, 
                Email, 
                Phone, 
                Address, 
                CreatedAt
            FROM Customers 
            WHERE CustomerId = @CustomerId";
        
        return await connection.QuerySingleOrDefaultAsync<Customer>(
            sql, 
            new { CustomerId = id }
        );
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            SELECT 
                CustomerId, 
                FullName, 
                Email, 
                Phone, 
                Address, 
                CreatedAt
            FROM Customers 
            ORDER BY FullName";
        
        return await connection.QueryAsync<Customer>(sql);
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            SELECT 
                CustomerId, 
                FullName, 
                Email, 
                Phone, 
                Address, 
                CreatedAt
            FROM Customers 
            WHERE Email = @Email";
        
        return await connection.QuerySingleOrDefaultAsync<Customer>(
            sql, 
            new { Email = email }
        );
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        using var connection = new SqlConnection(_connectionString);
        
        customer.CreatedAt = DateTime.UtcNow;
        
        var sql = @"
            INSERT INTO Customers (FullName, Email, Phone, Address, CreatedAt)
            VALUES (@FullName, @Email, @Phone, @Address, @CreatedAt);
            
            SELECT CAST(SCOPE_IDENTITY() as int)";
        
        var id = await connection.ExecuteScalarAsync<int>(sql, customer);
        customer.CustomerId = id;
        
        return customer;
    }

    public async Task<bool> UpdateAsync(Customer customer)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            UPDATE Customers 
            SET 
                FullName = @FullName,
                Email = @Email,
                Phone = @Phone,
                Address = @Address
            WHERE CustomerId = @CustomerId";
        
        var affectedRows = await connection.ExecuteAsync(sql, customer);
        
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = "DELETE FROM Customers WHERE CustomerId = @CustomerId";
        
        var affectedRows = await connection.ExecuteAsync(sql, new { CustomerId = id });
        
        return affectedRows > 0;
    }
}
