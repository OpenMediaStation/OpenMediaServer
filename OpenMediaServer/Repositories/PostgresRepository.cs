using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using Npgsql;
using NpgsqlTypes;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Models;
using OpenMediaServer.Models.FileInfo;
using OpenMediaServer.Models.Metadata;
using OpenMediaServer.Models.Progress;

namespace OpenMediaServer.Repositories;

public class PostgresRepository : IDataRepository
{
    private Task MigrationTask { get; set; }
    public PostgresRepository(ILogger<PostgresRepository> logger)
    {
        Logger = logger;
        
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = Globals.DB_Host,
            Port = Globals.DB_Port,
            Username = Globals.DB_User,
            Password = Globals.DB_Password,
            Database = "openmediaserver",
            Encoding = "UTF8"
#if DEBUG
            , 
            IncludeErrorDetail = true
#endif
        };
        
        ConnectionString = builder.ConnectionString;
        
        InitializeDatabase();
        MigrationTask = MigrateFilesToDatabase(); //TODO FIX/Improve!
    }

    private ILogger<PostgresRepository> Logger { get; set; }
    private string ConnectionString { get; set; }

    private async Task MigrateFilesToDatabase()
    {
        await ImportDataFromFileSystem<MetadataModel>(Path.Combine(Globals.ConfigFolder, "metadata"));
        await ImportDataFromFileSystem<InventoryItem>(Path.Combine(Globals.ConfigFolder, "inventory"),
            (fs, category) => {return category switch
            {
                "Movie" => JsonSerializer.Deserialize<IEnumerable<Movie>>(fs, Globals.JsonOptions),
                "Show" => JsonSerializer.Deserialize<IEnumerable<Show>>(fs, Globals.JsonOptions),
                "Episode" => JsonSerializer.Deserialize<IEnumerable<Episode>>(fs, Globals.JsonOptions),
                "Season" => JsonSerializer.Deserialize<IEnumerable<Season>>(fs, Globals.JsonOptions),
                "Audiobook" => JsonSerializer.Deserialize<IEnumerable<Audiobook>>(fs, Globals.JsonOptions),
                "Book" => JsonSerializer.Deserialize<IEnumerable<Book>>(fs, Globals.JsonOptions),
                _ => throw new NotSupportedException()
            };});
        await ImportDataFromFileSystem<FileInfoModel>(Path.Combine(Globals.ConfigFolder, "fileInfo"));
        var usersDir = Path.Combine(Globals.ConfigFolder, "users");
        foreach (var dir in Directory.Exists(usersDir) ? Directory.EnumerateDirectories(Path.Combine(Globals.ConfigFolder, "users")): [])
        {
            var userId = Path.GetDirectoryName(dir);
            await ImportDataFromFileSystem<Progress>(Path.Combine(dir, "progress"), (fs, _) =>
            {
                return JsonSerializer.Deserialize<IEnumerable<Progress>>(fs, Globals.JsonOptions)?.
                    Select(p =>
                {
                    p.UserId = userId;
                    return p;
                });
            });
            await ImportDataFromFileSystem<FavoriteInfo>(Path.Combine(dir, "favorites"),
                (fs, category) => JsonSerializer.Deserialize<IEnumerable<Guid>>(fs, Globals.JsonOptions)?.Select(inventoryId =>
                    new FavoriteInfo() { InventoryId = inventoryId, UserId = userId ?? throw new InvalidOperationException(), Id = Guid.NewGuid(), Category = category}));
            if (Directory.Exists(Path.Combine(dir, "bookmarks")))
            {
                foreach (var bmcatDir in Directory.EnumerateDirectories(Path.Combine(dir, "bookmarks")))
                {
                    await ImportDataFromFileSystem<Bookmark>(bmcatDir, (fs, invItemId) =>
                    {
                        return JsonSerializer.Deserialize<IEnumerable<Bookmark>>(fs, Globals.JsonOptions)?.Select(bm =>
                        {
                            bm.UserId = userId;
                            bm.InventoryItemId = Guid.TryParse(invItemId, out var invId) ? invId : null;
                            return bm;
                        });
                    });
                }
            }
        }
    }

    private async Task ImportDataFromFileSystem<T>(string dirPath, Func<FileStream, string, IEnumerable<T>?>? deserializeHandler = null)
    {
        if(!Directory.Exists(dirPath))
            return;
        var categoryFiles = Directory.EnumerateFiles(dirPath, "*.json", SearchOption.TopDirectoryOnly);
        foreach (var file in categoryFiles)
        {
            var category = Path.GetFileNameWithoutExtension(file);
            await using var fs = File.OpenRead(file);
            
            var items = deserializeHandler?.Invoke(fs,category) ?? JsonSerializer.Deserialize<IEnumerable<T>>(fs, Globals.JsonOptions);
            if (items is null)
            {
                Logger.LogCritical($"Unable to Deserialize {typeof(T).Name} Items of type: {category} during initial Database Migration");
                continue;
            }
            await BulkInsert(items);
            
            //Performing Clenup/backup by moving the files to an "imported" directory to have a backup in the worst case
            var backupDir = Path.Combine(Globals.ConfigFolder, "imported", typeof(T).Name);
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }
            File.Move(file, Path.Combine(backupDir, Path.GetFileName(file)));
        }
        
        //All files processed.. deleting directory
        Directory.Delete(dirPath, true);
    }
    
    private async Task BulkInsert<T>(IEnumerable<T> items)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMinutes(5));
        var cancelToken = cts.Token;
        var genericType = items.GetType().GenericTypeArguments.First();
        var tableName = GetTableName<T>();
        var columns = GetColumnNamesFromType(genericType);
        await connection.OpenAsync(cancelToken);
        await using var writer =
            await connection.BeginBinaryImportAsync(
                $"Copy {tableName} ({columns}) FROM STDIN (FORMAT BINARY)", cancelToken);
        foreach (var item in items)
        {
            await writer.StartRowAsync(cancellationToken: cancelToken);
            if (item == null) continue;
            foreach (var value in GetObjectPropertiesForInsert(item))
            {
                NpgsqlDbType dbType;
                if (!Enum.TryParse(value.typeName, ignoreCase: true, out dbType))
                {
                    Logger.LogWarning($"typeName not set or unable to find matching value for: {value.typeName} in {nameof(NpgsqlDbType)} enum using Unknown as fallback.");
                    dbType = NpgsqlDbType.Unknown;
                }
                await writer.WriteAsync(value.value, cancellationToken: cancelToken, npgsqlDbType: dbType);
            }
        }
        await writer.CompleteAsync(cancelToken);
    }
    
    private static (string typeName ,object value)[] GetObjectPropertiesForInsert(object obj)
    {
        var type = obj.GetType();
        if (type.BaseType != null && type.BaseType.Assembly == Assembly.GetExecutingAssembly())
            type = type.BaseType;
        var props = type.GetProperties().Where(p => p.CustomAttributes.Any(attr => attr.AttributeType == typeof(ColumnAttribute))).ToArray();
        var resultSet = new (string typeName, object value)[props.Length + 1];
        foreach (var prop in props)
        {
            var propValue = prop.GetValue(obj);
            var column = prop.GetCustomAttribute<ColumnAttribute>();
            var isColumn = column != null;
            if (isColumn)
            {
                resultSet[column!.Order] = (column.TypeName ,propValue!)!;
            }
        }
        resultSet[^1] = ("jsonb",JsonSerializer.Serialize(obj));
        return resultSet;
    }
    
    private static string GetColumnNamesFromType(Type type)
    {
        if (type.BaseType != null && type.BaseType.Assembly == Assembly.GetExecutingAssembly())
            type = type.BaseType;
        var columnProps = type.GetProperties().Where(p => p.CustomAttributes.Any(attr => attr.AttributeType == typeof(ColumnAttribute))).ToArray();
        var columnNames = columnProps.OrderBy(p => p.GetCustomAttribute<ColumnAttribute>()!.Order).Select(p => $"{p.Name}").Append("json_data");
        return string.Join(",", columnNames);
    }

    private void InitializeDatabase()
    {
        CreateDatabase();
        CreateTable<MetadataModel>();
        CreateTable<InventoryItem>();
        CreateTable<FileInfoModel>();
        CreateTable<Progress>();
        CreateTable<FavoriteInfo>();
        CreateTable<Bookmark>();
    }

    private void CreateTable<T>()
    {
        var columns = GetColumnDefinition<T>();
        var tableName = GetTableName<T>();
        
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        
        var tableExists = CheckIfTableExists(tableName, connection);
        if (!tableExists)
        {
            connection.Execute($"CREATE TABLE IF NOT EXISTS {tableName} ({columns})");
        }
        else
        {
            //TODO Implement table update!
            var existingColumns = GetExistingTableColumns(tableName, connection);
        }

        return;

        static bool CheckIfTableExists(string tableName, NpgsqlConnection connection)
        {
            var result = (int?)new NpgsqlCommand($"select 1 from pg_tables where tablename = '{tableName}'", connection).ExecuteScalar();
            return (result ?? 0) == 1;
        }

        static IEnumerable<(string ColumnName, string type)> GetExistingTableColumns(string tableName, NpgsqlConnection connection)
        {
            var command =
                new NpgsqlCommand(
                    $"select column_name, data_type, is_nullable, column_default from information_schema.columns where table_schema = 'public' and table_name = '{tableName}'", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                yield return (reader["column_name"].ToString()!, reader["type"].ToString()!);
            }
        }
    }

    private string GetColumnDefinition<T>()
    {
        return GetColumnDefinition(typeof(T));
    }
    private string GetColumnDefinition(Type type)
    {
        var props = type.GetProperties()
            .Where(p => p.CustomAttributes.Any(attr => attr.AttributeType == typeof(ColumnAttribute))).OrderBy(p => p.GetCustomAttribute<ColumnAttribute>()!.Order);

        var sb = new StringBuilder();
        
        foreach (var prop in props)
        {
            var columnName = prop.Name;
            var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
            var columnType = columnAttr!.TypeName;
            sb.Append($"{columnName} {columnType}");
            var isPrimaryKey = prop.GetCustomAttribute<KeyAttribute>() != null;
            if(isPrimaryKey)
                sb.Append(" PRIMARY KEY");
            var foreignKey = prop.GetCustomAttribute<ForeignKeyAttribute>();
            if(foreignKey != null)
                sb.Append($" REFERENCES t_{foreignKey.Name} ON DELETE CASCADE");
            
            sb.Append(", ");
        }

        sb.Append("json_data JSONB");

        return sb.ToString();
    }
    
    private void CreateDatabase()
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString: ConnectionString);
            var dbName = builder.Database;
            builder.Database = "postgres";
            using var connection = new NpgsqlConnection(builder.ConnectionString);
            connection.Open();

            connection.Execute($"CREATE DATABASE {dbName};");
        }
        catch (PostgresException e) when (e.SqlState == "42P04")
        {
            Logger.LogInformation("Database already exists.");
        }
        catch (Exception e)
        {
            Logger.LogCritical(e, "Unable to create database");
        }
    }


    
#region Public Methods
    public async Task WriteObjectsAsync<T>(IEnumerable<T> items)
    {
        await MigrationTask;
        if (items == null)
            return;
        
        var tableName = GetTableName<T>();
        var keyProp = typeof(T).GetProperties().FirstOrDefault(p => p.CustomAttributes.Any(attr => attr.AttributeType == typeof(KeyAttribute)));

        if(keyProp is null)
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
        
        var columnNameListWithoutKey = columnNames.Split(',').Select(n => n.Trim()).Where(n => !n.Equals(keyProp.Name, StringComparison.InvariantCultureIgnoreCase)).ToList();
        var updateDef = string.Join(", ", columnNameListWithoutKey.Select(n => $" {n} = EXCLUDED.{n}"));
        var whereClause = string.Join(" OR ", columnNameListWithoutKey.Select(n => $"{tableName}.{n} IS DISTINCT FROM EXCLUDED.{n}"));

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            $"INSERT INTO {tableName} ({columnNames}) VALUES {valuesString} ON CONFLICT ({keyProp.Name}) DO UPDATE SET {updateDef} WHERE {whereClause}");
    }

    public void WriteObjects<T>(IEnumerable<T> items)
    {
        var task = WriteObjectsAsync<T>(items);
        if(!task.Wait(TimeSpan.FromSeconds(30)))
            throw new TimeoutException("Timeout while waiting for WriteObjects to complete");
    }

    public async Task DeleteObjectAsync<T>(T item)
    {
        await MigrationTask;
        var type = typeof(T);
        var id = (Guid?)type.GetProperties().SingleOrDefault(p => p.CustomAttributes.Any(attr => attr.AttributeType == typeof(KeyAttribute)) && p.PropertyType == typeof(Guid))?.GetValue(item);
        if(id == null)
            throw new ArgumentException("Item does not have a valid KeyAttribute");

        await DeleteObjectAsync<T>((Guid)id);
    }

    public void DeleteObject<T>(T item)
    {
        var task = DeleteObjectAsync<T>(item);
        if(!task.Wait(TimeSpan.FromSeconds(30)))
           throw new TimeoutException("Timeout while waiting for DeleteObject to complete");
    }

    public async Task DeleteObjectsAsync<T>(IEnumerable<T> items)
    {
        await MigrationTask;
        var type = typeof(T);
        var keyProp = type.GetProperties().SingleOrDefault(p => p.CustomAttributes.Any(attr => attr.AttributeType == typeof(KeyAttribute)) && p.PropertyType == typeof(Guid));
        if(keyProp == null)
            throw new ArgumentException("Item does not have a valid KeyAttribute");
        var ids = items.Select(i => (Guid?)keyProp.GetValue(i));
        var tableName = GetTableName<T>();
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync($"delete from {tableName} where {keyProp.Name} in ({string.Join(",", ids.Where(id => id != null).Select(id => $"'{id?.ToString()}'"))})");
    }

    public void DeleteObjects<T>(IEnumerable<T> items)
    {
        var task = DeleteObjectsAsync<T>(items);
        if(!task.Wait(TimeSpan.FromSeconds(30)))
           throw new TimeoutException("Timeout while waiting for DeleteObjects to complete");
    }

    public async Task WriteObjectAsync<T>(T item)
    {
        await MigrationTask;
        if (item == null)
            return;
        
        var tableName = GetTableName<T>();
        var keyProp = typeof(T).GetProperties().FirstOrDefault(p => p.CustomAttributes.Any(attr => attr.AttributeType == typeof(KeyAttribute)));

        if(keyProp is null)
            throw new ArgumentException("Item does not have a valid KeyAttribute");
        
        var values = GetObjectPropertiesForInsert(item).Select(val => val.value);
        var valuesString = CreateValuesString(values);
        var columnNames = GetColumnNamesFromType(typeof(T));
        
        var columnNameListWithoutKey = columnNames.Split(',').Select(n => n.Trim()).Where(n => !n.Equals(keyProp.Name, StringComparison.InvariantCultureIgnoreCase)).ToList();
        var updateDef = string.Join(", ", columnNameListWithoutKey.Select(n => $" {n} = EXCLUDED.{n}"));
        var whereClause = string.Join(" OR ", columnNameListWithoutKey.Select(n => $"{tableName}.{n} IS DISTINCT FROM EXCLUDED.{n}"));

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            $"INSERT INTO {tableName} ({columnNames}) VALUES ({valuesString}) ON CONFLICT ({keyProp.Name}) DO UPDATE SET {updateDef} WHERE {whereClause}");
    }

    public void WriteObject<T>(T item)
    {
        var task = WriteObjectAsync<T>(item);
        if(!task.Wait(TimeSpan.FromSeconds(30)))
           throw new TimeoutException("Timeout while waiting for WriteObject to complete");
    }
    
    public async Task<IEnumerable<T>> ListObjectsAsync<T>(Expression<Func<T, bool>>? filter = null)
    {
        await MigrationTask;
        var tableName = GetTableName<T>();
        var filterExpression = ExpressionToSql(filter);
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        var results = new List<T>();
        await foreach (var item in connection.QueryAsync<T>($"select * from {tableName} {filterExpression}"))
        {
            if(item is null)
                continue;
            results.Add(item);
        }
        return results;
    }

    public IEnumerable<T> ListObjects<T>(Expression<Func<T, bool>>? filter = null)
    {
        var task = ListObjectsAsync(filter);
        if(!task.Wait(TimeSpan.FromSeconds(30)))
           throw new TimeoutException("Timeout while waiting for ListObjects to complete");
        return task.Result;
    }

    public async Task<IEnumerable<TO>> ListObjectsAsync<T, TO>(Expression<Func<T, TO>>? select = null)
    {
        await MigrationTask;
        //TODO Implement!
        throw new NotImplementedException();
    }

    public IEnumerable<TO> ListObjects<T, TO>(Expression<Func<T, TO>>? select = null)
    {
        var task = ListObjectsAsync<T, TO>(select);
        if(!task.Wait(TimeSpan.FromSeconds(30)))
           throw new TimeoutException("Timeout while waiting for ListObjects to complete");
        return task.Result;
    }
    
    public async Task<T?> GetObjectByIdAsync<T>(Guid id, Expression<Func<T, bool>>? additionalFilter = null)
    {
        await MigrationTask;
        var tableName = GetTableName<T>();
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        var results = new List<T?>();
        await foreach (var item in connection.QueryAsync<T>($"select * from {tableName} where id = '{id}'"))
        {
            results.Add(item);
        }
        return results.SingleOrDefault();
    }

    public T? GetObjectById<T>(Guid id, Expression<Func<T, bool>>? filter = null)
    {
        var task = GetObjectByIdAsync<T>(id);
        if(!task.Wait(TimeSpan.FromSeconds(30)))
           throw new TimeoutException("Timeout while waiting for GetObjectById to complete");
        return task.Result;
    }

    
    public async Task DeleteObjectAsync<T>(Guid id, Expression<Func<T, bool>>? filter = null)
    {
        await MigrationTask;
        var sqlFilter = filter != null ? ExpressionToSql(filter).Replace("WHERE", "AND") : string.Empty;
        var tableName = GetTableName<T>();
        using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
        {
            await connection.OpenAsync();
            var res = await connection.ExecuteAsync($"delete from {tableName} where id = {id} {sqlFilter}");
            if (res != 1)
            {
                //TODO Maybe throw exception!
            }
        }
    }

    public void DeleteObject<T>(Guid id, Expression<Func<T, bool>>? filter = null)
    {
        var task = DeleteObjectAsync<T>(id, filter);
        if(!task.Wait(TimeSpan.FromSeconds(30)))
           throw new TimeoutException("Timeout while waiting for DeleteObject to complete");
    }
    #endregion

    
    # region Static Private Methods
    private static string ExpressionToSql(LambdaExpression? expression)
    {
        if (expression == null)
            return string.Empty;

        var body = expression.Body;

        var sb = new StringBuilder();
        sb.Append($" WHERE ");
        VisitExpression(body);

        return sb.ToString();

        void VisitExpression(Expression expr)
        {
            switch (expr)
            {
                case BinaryExpression binary:
                    sb.Append('(');
                    VisitExpression(binary.Left);
                    sb.Append(' ');
                    sb.Append(GetSqlOperator(binary.NodeType));
                    sb.Append(' ');
                    VisitExpression(binary.Right);
                    sb.Append(')');
                    break;

                case MemberExpression member:
                    if (member.Expression is ParameterExpression)
                    {
                        sb.Append(member.Member.Name);
                    }
                    else// z. B. captured variable
                    {
                        var value = GetValueFromExpression(member);
                        AppendSqlValue(value, sb);
                    }
                    break;

                case ConstantExpression constant:
                    AppendSqlValue(constant.Value, sb);
                    break;

                case UnaryExpression unary when unary.NodeType == ExpressionType.Convert:
                    VisitExpression(unary.Operand);
                    break;

                default:
                    var val = GetValueFromExpression(expr);
                    AppendSqlValue(val, sb);
                    break;
            }
        }
        static string GetSqlOperator(ExpressionType nodeType)
        {
            return nodeType switch
            {
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "!=",
                ExpressionType.AndAlso => "AND",
                ExpressionType.OrElse => "OR",
                ExpressionType.GreaterThan => ">",
                ExpressionType.LessThan => "<",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThanOrEqual => "<=",
                _ => throw new NotSupportedException($"Operator '{nodeType}' wird nicht unterstützt."),
            };
        }
        static object? GetValueFromExpression(Expression expr)
        {
            try
            {
                var lambda = Expression.Lambda(expr);
                var compiled = lambda.Compile();
                return compiled.DynamicInvoke();
            }
            catch
            {
                return null;
            }
        }

    }
        
    private static void AppendSqlValue(object? value, StringBuilder sb)
    {
        switch (value)
        {
            case null:
                sb.Append("NULL");
                break;
            case string:
            case Guid:
                sb.Append('\'');
                sb.Append(value.ToString()?.Replace("'", "''"));
                sb.Append('\'');
                break;
            case bool b:
                sb.Append(b ? "TRUE" : "FALSE");
                break;
            default:
                sb.Append(value);
                break;
        }
    }

    private static string CreateValuesString(IEnumerable<object> values)
    {
        var sb = new StringBuilder();
        foreach (var value in values)
        {
            AppendSqlValue(value, sb);
            sb.Append(", ");
        }

        return sb.ToString().Trim().TrimEnd(',');
    }
    
    private static string GetTableName(Type type)
    {
        if (type.BaseType != null && type.BaseType.Assembly == Assembly.GetExecutingAssembly())
            type = type.BaseType;
        var className = type.Name;
        var tableName = "t_" + className;
        return tableName.ToLower();
    }
    
    private static string GetTableName<T>()
    {
        return GetTableName(typeof(T));
    }

    #endregion
}

public static class PgSqlExtensions
{
    public static int Execute(this NpgsqlConnection connection, string sql, params object[] parameters)
    {
        using var command = new NpgsqlCommand(sql, connection);
        // command.Parameters.AddRange(parameters.Select(p => new NpgsqlParameter(p.GetType().Name, p)).ToArray());
        return command.ExecuteNonQuery();
    }
    
    public static async Task<int> ExecuteAsync(this NpgsqlConnection connection, string sql, params object[] parameters)
    {
        using var command = new NpgsqlCommand(sql, connection);
        // command.Parameters.AddRange(parameters.Select(p => new NpgsqlParameter(p.GetType().Name, p)).ToArray());
        return await command.ExecuteNonQueryAsync();
    }

    //TODO Maybe use IAsyncEnumerable<T?>
    public static async IAsyncEnumerable<T?> QueryAsync<T>(this NpgsqlConnection connection, string sql, params object[] parameters)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        // command.Parameters.AddRange(parameters.Select(p => new NpgsqlParameter(p.GetType().Name, p)).ToArray());
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            yield return JsonSerializer.Deserialize<T>(reader["json_data"].ToString() ?? string.Empty);
        }
    }
}