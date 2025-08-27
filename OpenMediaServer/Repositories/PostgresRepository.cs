using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Npgsql;
using OpenMediaServer.Extensions;
using OpenMediaServer.Helpers;
using OpenMediaServer.Interfaces.Database;
using OpenMediaServer.Interfaces.Repositories;

namespace OpenMediaServer.Repositories;

public class PostgresRepository : IDataRepository
{
    private readonly ILogger<PostgresRepository> _logger;
    private readonly string _connectionString;

    public PostgresRepository(ILogger<PostgresRepository> logger, IPostgresManager manager)
    {
        _logger = logger;

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = Globals.DB_Host,
            Port = Globals.DB_Port,
            Username = Globals.DB_User,
            Password = Globals.DB_Password,
            Database = "openmediaserver",
            Encoding = "UTF8",
#if DEBUG
            IncludeErrorDetail = true,
#endif
        };

        _connectionString = builder.ConnectionString;

        manager.InitializeDatabase(_connectionString);
    }
    
    public async Task WriteObject<T>(T item)
    {
        await WriteObjects([item]);
    }

    public async Task WriteObjects<T>(IEnumerable<T>? items)
    {
        if (items == null)
            return;

        var tableName = ExpressionToSqlConverter.GetTableName(typeof(T));
        var keyProp = typeof(T).GetProperties()
            .FirstOrDefault(p => p.CustomAttributes.Any(attr => attr.AttributeType == typeof(KeyAttribute)));

        if (keyProp is null)
            throw new ArgumentException("ItemType does not have a valid KeyAttribute");

        var sb = new StringBuilder();
        foreach (var item in items)
        {
            if (item == null)
                continue;
            sb.Append('(');
            var values = GetObjectPropertiesForInsert(item).Select(val => val.value);
            var itemValuesString = CreateValuesString(values);
            sb.Append(itemValuesString);
            sb.Append("),");
        }

        var valuesString = sb.ToString().TrimEnd(',');
        var columnNames = GetColumnNamesFromType(typeof(T));

        var columnNameListWithoutKey = columnNames.Split(',').Select(n => n.Trim())
            .Where(n => !n.Equals(keyProp.Name, StringComparison.InvariantCultureIgnoreCase)).ToList();
        var updateDef = string.Join(", ", columnNameListWithoutKey.Select(n => $" {n} = EXCLUDED.{n}"));
        var whereClause = string.Join(" OR ",
            columnNameListWithoutKey.Select(n => $"{tableName}.{n} IS DISTINCT FROM EXCLUDED.{n}"));

        var query = $"INSERT INTO {tableName} ({columnNames}) VALUES {valuesString} ON CONFLICT ({keyProp.Name}) DO UPDATE SET {updateDef} WHERE {whereClause}";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(query);
    }

    public async Task DeleteObjectWithFilter<T>(Guid id, Expression<Func<T, bool>>? filter = null)
    {
        var sqlFilter = filter != null
            ? ExpressionToSqlConverter.ExpressionToSql(filter).Replace("WHERE", "AND")
            : string.Empty;
        var tableName = ExpressionToSqlConverter.GetTableName(typeof(T));
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        var res = await connection.ExecuteAsync($"delete from {tableName} where id = \'{id}\' {sqlFilter}");
        if (res != 1)
        {
            //TODO Maybe throw exception!
        }
    }    
    
    public async Task DeleteObjectWithFilter<T>(Expression<Func<T, bool>>? filter = null)
    {
        var sqlFilter = filter != null
            ? ExpressionToSqlConverter.ExpressionToSql(filter)
            : string.Empty;
        var tableName = ExpressionToSqlConverter.GetTableName(typeof(T));
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        var res = await connection.ExecuteAsync($"delete from {tableName} {sqlFilter}");
        if (res != 1)
        {
            //TODO Maybe throw exception!
        }
    }
    
    public async Task DeleteObjects<T>(IEnumerable<T> items)
    {
        var type = typeof(T);
        var keyProp = type.GetProperties().SingleOrDefault(p =>
            p.CustomAttributes.Any(attr => attr.AttributeType == typeof(KeyAttribute)) &&
            p.PropertyType == typeof(Guid));
        if (keyProp == null)
            throw new ArgumentException("Item does not have a valid KeyAttribute");
        var ids = items.Select(i => (Guid?)keyProp.GetValue(i)).ToList();

        if (ids.Count == 0)
        {
            return;
        }
        
        var tableName = ExpressionToSqlConverter.GetTableName(typeof(T));
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            $"delete from {tableName} where {keyProp.Name} in ({string.Join(",", ids.Where(id => id != null).Select(id => $"'{id?.ToString()}'"))})");
    }

    public async Task<IEnumerable<T>> ListObjects<T>(Expression<Func<T, bool>>? filter = null)
    {
        var tableName = ExpressionToSqlConverter.GetTableName(typeof(T));
        var filterExpression = ExpressionToSqlConverter.ExpressionToSql(filter);
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        var results = new List<T>();
        await foreach (var item in connection.QueryAsync<T>($"select * from {tableName} {filterExpression}"))
        {
            if (item is null)
                continue;
            results.Add(item);
        }

        return results;
    }

    public async Task<T?> GetObjectById<T>(Guid? id, Expression<Func<T, bool>>? additionalFilter = null)
    {
        if (id == null)
        {
            return default;
        }
        
        var tableName = ExpressionToSqlConverter.GetTableName(typeof(T));
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        var results = new List<T?>();
        await foreach (var item in connection.QueryAsync<T>($"select * from {tableName} where id = '{id}'"))
        {
            results.Add(item);
        }

        return results.SingleOrDefault();
    }




    private static (string typeName, object value)[] GetObjectPropertiesForInsert(object obj)
    {
        var type = obj.GetType();
        if (type.BaseType != null && type.BaseType.Assembly == Assembly.GetExecutingAssembly())
            type = type.BaseType;

        var props = type.GetProperties()
            .Where(p => p.GetCustomAttribute<ColumnAttribute>() != null)
            .ToArray();

        var resultList = new List<(string typeName, object value)>();

        foreach (var prop in props)
        {
            var propValue = prop.GetValue(obj);
            var column = prop.GetCustomAttribute<ColumnAttribute>();
            resultList.Add((column!.TypeName, propValue!)!);
        }
        
        return resultList.ToArray();
    }
    
    private static string GetColumnNamesFromType(Type type)
    {
        if (type.BaseType != null && type.BaseType.Assembly == Assembly.GetExecutingAssembly())
            type = type.BaseType;
        var columnProps = type.GetProperties()
            .Where(p => p.CustomAttributes.Any(attr => attr.AttributeType == typeof(ColumnAttribute))).ToArray();
        var columnNames = columnProps //.OrderBy(p => p.GetCustomAttribute<ColumnAttribute>()!.Order)
            .Select(p => $"{p.Name}");
        return string.Join(",", columnNames);
    }

    private static string CreateValuesString(IEnumerable<object> values)
    {
        var sb = new StringBuilder();
        foreach (var value in values)
        {
            ExpressionToSqlConverter.AppendSqlValue(value, sb);
            sb.Append(", ");
        }

        return sb.ToString().Trim().TrimEnd(',');
    }
}