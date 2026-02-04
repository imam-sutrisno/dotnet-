using Dapper;
using Microsoft.Data.SqlClient;
using ProductAPI.Domain.Entities;
using ProductAPI.Domain.Repositories;

namespace ProductAPI.Infrastructure.DataAccess.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            SELECT 
                OrderId, 
                CustomerId, 
                OrderDate, 
                TotalAmount, 
                Status
            FROM Orders 
            WHERE OrderId = @OrderId";
        
        return await connection.QuerySingleOrDefaultAsync<Order>(
            sql, 
            new { OrderId = id }
        );
    }

    public async Task<Order?> GetByIdWithDetailsAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            SELECT 
                o.OrderId, 
                o.CustomerId, 
                o.OrderDate, 
                o.TotalAmount, 
                o.Status,
                c.CustomerId, 
                c.FullName, 
                c.Email, 
                c.Phone, 
                c.Address,
                oi.OrderItemId, 
                oi.OrderId, 
                oi.ProductId, 
                p.Name as ProductName,
                oi.Quantity, 
                oi.UnitPrice,
                oi.TotalPrice
            FROM Orders o
            INNER JOIN Customers c ON o.CustomerId = c.CustomerId
            LEFT JOIN OrderItems oi ON o.OrderId = oi.OrderId
            LEFT JOIN Products p ON oi.ProductId = p.Id
            WHERE o.OrderId = @OrderId";
        
        var orderDict = new Dictionary<int, Order>();
        
        await connection.QueryAsync<Order, Customer, OrderItem, Order>(
            sql,
            (order, customer, orderItem) =>
            {
                if (!orderDict.TryGetValue(order.OrderId, out var currentOrder))
                {
                    currentOrder = order;
                    currentOrder.Customer = customer;
                    orderDict.Add(order.OrderId, currentOrder);
                }
                
                if (orderItem != null && orderItem.OrderItemId > 0)
                {
                    currentOrder.Items.Add(orderItem);
                }
                
                return currentOrder;
            },
            new { OrderId = id },
            splitOn: "CustomerId,OrderItemId"
        );
        
        return orderDict.Values.FirstOrDefault();
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            SELECT 
                OrderId, 
                CustomerId, 
                OrderDate, 
                TotalAmount, 
                Status
            FROM Orders 
            WHERE CustomerId = @CustomerId
            ORDER BY OrderDate DESC";
        
        return await connection.QueryAsync<Order>(
            sql, 
            new { CustomerId = customerId }
        );
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            SELECT 
                OrderId, 
                CustomerId, 
                OrderDate, 
                TotalAmount, 
                Status
            FROM Orders 
            ORDER BY OrderDate DESC";
        
        return await connection.QueryAsync<Order>(sql);
    }

    public async Task<Order> CreateAsync(Order order)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        using var transaction = connection.BeginTransaction();
        
        try
        {
            order.OrderDate = DateTime.UtcNow;
            
            // Insert Order
            var orderSql = @"
                INSERT INTO Orders (CustomerId, OrderDate, TotalAmount, Status)
                VALUES (@CustomerId, @OrderDate, @TotalAmount, @Status);
                
                SELECT CAST(SCOPE_IDENTITY() as int)";
            
            var orderId = await connection.ExecuteScalarAsync<int>(
                orderSql, 
                order, 
                transaction
            );
            
            order.OrderId = orderId;
            
            // Insert Order Items
            if (order.Items.Any())
            {
                var itemsSql = @"
                    INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, TotalPrice)
                    VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @TotalPrice)";
                
                foreach (var item in order.Items)
                {
                    item.OrderId = orderId;
                    item.TotalPrice = item.Quantity * item.UnitPrice;
                    
                    await connection.ExecuteAsync(itemsSql, item, transaction);
                }
            }
            
            transaction.Commit();
            return order;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(int orderId, string status)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            UPDATE Orders 
            SET Status = @Status
            WHERE OrderId = @OrderId";
        
        var affectedRows = await connection.ExecuteAsync(
            sql, 
            new { OrderId = orderId, Status = status }
        );
        
        return affectedRows > 0;
    }
}
