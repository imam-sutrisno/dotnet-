using System.Data;

namespace ProductAPI.Infrastructure.DataAccess.Context;

/// <summary>
/// Interface for database connection factory
/// Provides abstraction for creating database connections
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates a new database connection
    /// </summary>
    /// <returns>A new IDbConnection instance</returns>
    IDbConnection CreateConnection();
}
