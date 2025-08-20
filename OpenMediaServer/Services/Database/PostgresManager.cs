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

        var tables = new List<Type>()
        {
            typeof(InventoryItem),
            typeof(FileInfoModel),
            typeof(InventoryItemVersion),
            typeof(Progress),
            typeof(FavoriteInfo),
            typeof(Bookmark),
            typeof(InventoryItemAddon),
            typeof(InventoryItemPart),
            typeof(MetadataModel),
            typeof(MetadataAudiobookModel),
            typeof(MetadataBookModel),
            typeof(MetadataEpisodeModel),
            typeof(MetadataMovieModel),
            typeof(MetadataSeasonModel),
            typeof(MetadataShowModel),
            typeof(MetadataChapter),
            typeof(VideoStream),
            typeof(SubtitleStream),
            typeof(AudioStream),
            typeof(MediaData),
            typeof(MediaFormat)
        };

        CreateTables(connectionString, tables);
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

    private void CreateTables(string connectionString, List<Type> tableTypes)
    {
        foreach (var tableType in tableTypes)
        {
            var columns = GetColumnDefinition(tableType);
            var tableName = ExpressionToSqlConverter.GetTableName(tableType);

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
                var expectedColumns = GetColumnDefinition(tableType).Split(',').Select(cd => cd.Trim());

                foreach (var columnDef in expectedColumns.Where(c =>
                             !existingColumns.Any(col =>
                                 c.StartsWith(col.ColumnName, StringComparison.InvariantCultureIgnoreCase))))
                {
                    connection.Execute($"ALTER TABLE {tableName} ADD COLUMN IF NOT EXISTS {columnDef.Trim()}");
                    var columnName = columnDef.Split(' ').First();
                    connection.Execute(
                        $"update {tableName} set {columnName} = COALESCE({columnName}) where {columnName} is null");
                }
            }
        }

        foreach (var tableType in tableTypes)
        {
            var constraints = GetConstraintDefinition(tableType);

            if (string.IsNullOrWhiteSpace(constraints))
            {
                continue;
            }
            
            var tableName = ExpressionToSqlConverter.GetTableName(tableType);
            
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            
            var tableExists = CheckIfTableExists(tableName, connection);
            
            if (tableExists)
            {
                connection.Execute($"ALTER TABLE {tableName} {constraints}");
            }
        }
    }

    private string GetColumnDefinition(Type type)
    {
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

            sb.Append(", ");
        }

        if (sb.Length > 2)
        {
            sb.Remove(sb.Length - 2, 2);
        }
        
        return sb.ToString();
    }

    private string GetConstraintDefinition(Type type)
    {
        var props = type.GetProperties()
            .Where(p => p.CustomAttributes.Any(attr => attr.AttributeType == typeof(ColumnAttribute)))
            .OrderBy(p => p.GetCustomAttribute<ColumnAttribute>()!.Order);

        var sb = new StringBuilder();

        foreach (var prop in props)
        {
            var foreignKey = prop.GetCustomAttribute<ForeignKeyAttribute>();

            if (foreignKey != null)
            {
                var columnName = prop.Name;
                var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
                var columnType = columnAttr!.TypeName;

                sb.Append($"ADD FOREIGN KEY ({columnName})");
                
                sb.Append($" REFERENCES t_{foreignKey.Name} ON DELETE CASCADE");

                sb.Append(", ");
            }
        }

        if (sb.Length > 2)
        {
            sb.Remove(sb.Length - 2, 2);
        }
        
        return sb.ToString();
    }

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