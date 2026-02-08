using System.Data;
using MySqlConnector;

namespace ProductAPI.Infrastructure.DataAccess.Context;

/// <summary>
/// MySQL implementation of IDbConnectionFactory
/// Creates MySqlConnection instances for database operations
/// </summary>
public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));
        
        _connectionString = connectionString;
    }

    /// <summary>
    /// Creates a new MySQL connection
    /// </summary>
    /// <returns>A new MySqlConnection instance</returns>
    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}
