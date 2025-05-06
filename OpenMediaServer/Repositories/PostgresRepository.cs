using System.Linq.Expressions;
using Dapper;
using Npgsql;
using OpenMediaServer.Interfaces.Repositories;

namespace OpenMediaServer.Repositories;

public class PostgresRepository : IDataRepository
{
    public string ConnectionString { get; set; }
    public PostgresRepository(string connectionString)
    {
        ConnectionString = connectionString;
    }


    public async Task WriteObject<T>(T item)
    {
        var tableName = GetTableName<T>();
        var id = typeof(T).GetProperties().FirstOrDefault(p => p.Name.Equals("id", StringComparison.InvariantCultureIgnoreCase) && p.PropertyType == typeof(Guid))?.GetValue(item);
        
        await using (var connection = new NpgsqlConnection(ConnectionString))
        {
            await connection.OpenAsync();
            var queryRes = await connection.QueryAsync<T>($"select * from {tableName} {(id != null ? ($"where id = {id}") : string.Empty)}");
            if (queryRes.SingleOrDefault() != null)
            {
                // var lel = await connection.ExecuteAsync("INSERT INTO {tableName} ({columns}) VALUES ({values}) ON CONFLICT (id) DO UPDATE SET {updates}");
            }
        }
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<T?>> ListObjects<T>(Expression<Func<T, bool>>? filter = null)
    {
        var tableName = GetTableName<T>();
        var filterExpression = ExpressionToSql(filter);
        await using (var connection = new NpgsqlConnection(ConnectionString))
        {
            await connection.OpenAsync();
            var queryRes = await connection.QueryAsync<T>($"select * from {tableName} {filterExpression}");
            return queryRes;
        }
    }
    
    public async Task<T?> GetObjectByID<T>(Guid id)
    {
        var tableName = GetTableName<T>();
        using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
        {
            await connection.OpenAsync();
            var queryRes = await connection.QueryAsync<T>($"select * from {tableName} where id = {id}");
            return queryRes.SingleOrDefault();
        }
    }

    
    public async Task DeleteObject<T>(Guid id)
    {
        var tableName = GetTableName<T>();
        using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
        {
            await connection.OpenAsync();
            var res = await connection.ExecuteAsync($"delete from {tableName} where id = {id}");
            if (res != 1)
            {
                //TODO Maybe throw exception!
            }
        }
    }

    private string ExpressionToSql(Expression? expression)
    {
        if (expression == null)
            return string.Empty;
        
        var lel = expression.ToString();
        //TODO complete/fix this
        throw new NotImplementedException(); 
    }

    private string GetTableName(Type type)
    {
        var className = type.Name;
        var tableName = "t_" + className;
        return tableName;
    }

    private string GetTableName<T>()
    {
        return GetTableName(typeof(T));
    }

}