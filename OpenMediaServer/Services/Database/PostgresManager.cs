using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using Npgsql;
using OpenMediaServer.Extensions;
using OpenMediaServer.Helpers;
using OpenMediaServer.Interfaces.Database;
using OpenMediaServer.Models;
using OpenMediaServer.Models.FileInfo;
using OpenMediaServer.Models.Inventory;
using OpenMediaServer.Models.Metadata;
using OpenMediaServer.Models.Progress;

namespace OpenMediaServer.Services.Database;

public class PostgresManager(ILogger<PostgresManager> logger) : IPostgresManager
{
    public void InitializeDatabase(string connectionString)
    {
        CreateDatabase(connectionString);


        CreateTable<MetadataModel>(connectionString);
        CreateTable<InventoryItem>(connectionString);
        CreateTable<FileInfoModel>(connectionString);
        CreateTable<InventoryItemVersion>(connectionString);
        CreateTable<Progress>(connectionString);
        CreateTable<FavoriteInfo>(connectionString);
        CreateTable<Bookmark>(connectionString);
        CreateTable<InventoryItemAddon>(connectionString);
    }

    private string GetColumnDefinition<T>()
    {
        var type = typeof(T);

        var props = type.GetProperties()
            .Where(p => p.CustomAttributes.Any(attr => attr.AttributeType == typeof(ColumnAttribute)))
            .OrderBy(p => p.GetCustomAttribute<ColumnAttribute>()!.Order);

        var sb = new StringBuilder();

        foreach (var prop in props)
        {
            var columnName = prop.Name;
            var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
            var columnType = columnAttr!.TypeName;
            sb.Append($"{columnName} {columnType}");
            var isPrimaryKey = prop.GetCustomAttribute<KeyAttribute>() != null;
            if (isPrimaryKey)
                sb.Append(" PRIMARY KEY");
            var foreignKey = prop.GetCustomAttribute<ForeignKeyAttribute>();
            if (foreignKey != null)
                sb.Append($" REFERENCES t_{foreignKey.Name} ON DELETE CASCADE");

            sb.Append(", ");
        }

        sb.Append("json_data JSONB");

        return sb.ToString();
    }

    private void CreateDatabase(string connectionString)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString: connectionString);
            var dbName = builder.Database;
            builder.Database = "postgres";
            using var connection = new NpgsqlConnection(builder.ConnectionString);
            connection.Open();

            connection.Execute($"CREATE DATABASE {dbName};");
        }
        catch (PostgresException e) when (e.SqlState == "42P04")
        {
            logger.LogInformation("Database already exists.");
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Unable to create database");
        }
    }

    private void CreateTable<T>(string connectionString)
    {
        var columns = GetColumnDefinition<T>();
        var tableName = ExpressionToSqlConverter.GetTableName<T>();

        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        var tableExists = CheckIfTableExists(tableName, connection);
        if (!tableExists)
        {
            connection.Execute($"CREATE TABLE IF NOT EXISTS {tableName} ({columns})");
        }
        else
        {
            //TODO Implement table update!
            var existingColumns = GetExistingTableColumns(tableName, connection).ToList();
            var expectedColumns = GetColumnDefinition<T>().Split(',').Select(cd => cd.Trim());
            foreach (var columnDef in expectedColumns.Where(c =>
                         !existingColumns.Any(col =>
                             c.StartsWith(col.ColumnName, StringComparison.InvariantCultureIgnoreCase))))
            {
                connection.Execute($"ALTER TABLE {tableName} ADD COLUMN IF NOT EXISTS {columnDef.Trim()}");
                var columnName = columnDef.Split(' ').First();
                connection.Execute(
                    $"update {tableName} set {columnName} = COALESCE({columnName}, json_data->>'{columnName}') where {columnName} is null and json_data ? '{columnName}'");
            }
        }

        return;

        static bool CheckIfTableExists(string tableName, NpgsqlConnection connection)
        {
            var result = (int?)new NpgsqlCommand($"select 1 from pg_tables where tablename = '{tableName}'", connection)
                .ExecuteScalar();
            return (result ?? 0) == 1;
        }

        static IEnumerable<(string ColumnName, string type)> GetExistingTableColumns(string tableName,
            NpgsqlConnection connection)
        {
            var command =
                new NpgsqlCommand(
                    $"select column_name, data_type, is_nullable, column_default from information_schema.columns where table_schema = 'public' and table_name = '{tableName}'",
                    connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                yield return (reader["column_name"].ToString()!, reader["data_type"].ToString()!);
            }
        }
    }
}