using System.Data;
using Dapper;
using ProductAPI.Infrastructure.DataAccess.Context;

namespace ProductAPI.Infrastructure.DataAccess.Examples;

/// <summary>
/// Advanced Dapper examples based on learndapper.com
/// Demonstrates various Dapper features and best practices
/// </summary>
public class DapperExamples
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DapperExamples(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    #region Query Examples (from learndapper.com)

    /// <summary>
    /// Example 1: Query - Returns multiple rows
    /// https://www.learndapper.com/dapper-query
    /// </summary>
    public async Task<IEnumerable<T>> QueryExampleAsync<T>(string sql, object? parameters = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<T>(sql, parameters);
    }

    /// <summary>
    /// Example 2: QueryFirst - Returns first row, throws exception if no rows
    /// https://www.learndapper.com/dapper-query#queryfirst
    /// </summary>
    public async Task<T> QueryFirstExampleAsync<T>(string sql, object? parameters = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstAsync<T>(sql, parameters);
    }

    /// <summary>
    /// Example 3: QueryFirstOrDefault - Returns first row or default value
    /// https://www.learndapper.com/dapper-query#queryfirstordefault
    /// </summary>
    public async Task<T?> QueryFirstOrDefaultExampleAsync<T>(string sql, object? parameters = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
    }

    /// <summary>
    /// Example 4: QuerySingle - Returns single row, throws exception if not exactly one
    /// https://www.learndapper.com/dapper-query#querysingle
    /// </summary>
    public async Task<T> QuerySingleExampleAsync<T>(string sql, object? parameters = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<T>(sql, parameters);
    }

    /// <summary>
    /// Example 5: QuerySingleOrDefault - Returns single row or default
    /// https://www.learndapper.com/dapper-query#querysingleordefault
    /// </summary>
    public async Task<T?> QuerySingleOrDefaultExampleAsync<T>(string sql, object? parameters = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<T>(sql, parameters);
    }

    #endregion

    #region Execute Examples

    /// <summary>
    /// Example 6: Execute - For INSERT, UPDATE, DELETE
    /// https://www.learndapper.com/dapper-execute
    /// Returns number of rows affected
    /// </summary>
    public async Task<int> ExecuteExampleAsync(string sql, object? parameters = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, parameters);
    }

    /// <summary>
    /// Example 7: ExecuteScalar - Returns single value
    /// https://www.learndapper.com/dapper-execute#executescalar
    /// </summary>
    public async Task<T> ExecuteScalarExampleAsync<T>(string sql, object? parameters = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<T>(sql, parameters);
    }

    #endregion

    #region Parameter Examples

    /// <summary>
    /// Example 8: Anonymous Parameters
    /// https://www.learndapper.com/parameters#anonymous-parameters
    /// </summary>
    public async Task<IEnumerable<T>> AnonymousParametersExampleAsync<T>(string sql, int id, string name)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<T>(sql, new { Id = id, Name = name });
    }

    /// <summary>
    /// Example 9: DynamicParameters - For complex parameter scenarios
    /// https://www.learndapper.com/parameters#dynamic-parameters
    /// </summary>
    public async Task<T> DynamicParametersExampleAsync<T>(string sql, Dictionary<string, object> parameters)
    {
        using var connection = _connectionFactory.CreateConnection();
        var dynamicParams = new DynamicParameters();
        
        foreach (var param in parameters)
        {
            dynamicParams.Add(param.Key, param.Value);
        }
        
        return await connection.QuerySingleAsync<T>(sql, dynamicParams);
    }

    /// <summary>
    /// Example 10: List Parameters - For IN clause
    /// https://www.learndapper.com/parameters#list-parameters
    /// </summary>
    public async Task<IEnumerable<T>> ListParametersExampleAsync<T>(string sql, List<int> ids)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<T>(sql, new { Ids = ids });
    }

    #endregion

    #region Multi-Mapping Examples

    /// <summary>
    /// Example 11: One-to-One Mapping
    /// https://www.learndapper.com/relationships#one-to-one
    /// </summary>
    public async Task<IEnumerable<TParent>> OneToOneMappingAsync<TParent, TChild>(
        string sql, 
        Func<TParent, TChild, TParent> map,
        object? parameters = null,
        string splitOn = "Id")
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<TParent, TChild, TParent>(
            sql, 
            map, 
            parameters, 
            splitOn: splitOn
        );
    }

    /// <summary>
    /// Example 12: One-to-Many Mapping
    /// https://www.learndapper.com/relationships#one-to-many
    /// </summary>
    public async Task<IEnumerable<TParent>> OneToManyMappingAsync<TParent, TChild>(
        string sql,
        Func<TParent, int> getParentId,
        Action<TParent, TChild> addChild,
        object? parameters = null,
        string splitOn = "Id")
        where TParent : class
    {
        using var connection = _connectionFactory.CreateConnection();
        var lookup = new Dictionary<int, TParent>();

        await connection.QueryAsync<TParent, TChild, TParent>(
            sql,
            (parent, child) =>
            {
                var parentId = getParentId(parent);
                if (!lookup.TryGetValue(parentId, out var currentParent))
                {
                    currentParent = parent;
                    lookup.Add(parentId, currentParent);
                }

                if (child != null)
                {
                    addChild(currentParent, child);
                }

                return currentParent;
            },
            parameters,
            splitOn: splitOn
        );

        return lookup.Values;
    }

    #endregion

    #region Transaction Examples

    /// <summary>
    /// Example 13: Simple Transaction
    /// https://www.learndapper.com/transactions
    /// </summary>
    public async Task<bool> TransactionExampleAsync(
        List<(string sql, object parameters)> operations)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        
        using var transaction = connection.BeginTransaction();
        
        try
        {
            foreach (var (sql, parameters) in operations)
            {
                await connection.ExecuteAsync(sql, parameters, transaction);
            }
            
            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    #endregion

    #region Multiple Result Sets

    /// <summary>
    /// Example 14: QueryMultiple - Execute multiple queries in one go
    /// https://www.learndapper.com/multiple-resultsets
    /// </summary>
    public async Task<(IEnumerable<T1>, IEnumerable<T2>)> QueryMultipleExampleAsync<T1, T2>(
        string sql, 
        object? parameters = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(sql, parameters);
        
        var result1 = await multi.ReadAsync<T1>();
        var result2 = await multi.ReadAsync<T2>();
        
        return (result1, result2);
    }

    /// <summary>
    /// Example 15: QueryMultiple with three result sets
    /// </summary>
    public async Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> QueryMultipleThreeAsync<T1, T2, T3>(
        string sql, 
        object? parameters = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(sql, parameters);
        
        var result1 = await multi.ReadAsync<T1>();
        var result2 = await multi.ReadAsync<T2>();
        var result3 = await multi.ReadAsync<T3>();
        
        return (result1, result2, result3);
    }

    #endregion

    #region Stored Procedures

    /// <summary>
    /// Example 16: Execute Stored Procedure
    /// https://www.learndapper.com/stored-procedures
    /// </summary>
    public async Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(
        string procedureName, 
        object? parameters = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<T>(
            procedureName, 
            parameters, 
            commandType: CommandType.StoredProcedure
        );
    }

    /// <summary>
    /// Example 17: Stored Procedure with Output Parameters
    /// </summary>
    public async Task<T> StoredProcedureWithOutputAsync<T>(
        string procedureName,
        DynamicParameters parameters)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            procedureName, 
            parameters, 
            commandType: CommandType.StoredProcedure
        );
        
        return parameters.Get<T>("@OutputParameter");
    }

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Example 18: Bulk Insert using Execute
    /// https://www.learndapper.com/bulk-operations
    /// </summary>
    public async Task<int> BulkInsertAsync<T>(string sql, IEnumerable<T> entities)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, entities);
    }

    #endregion
}
