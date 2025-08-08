using System.Text.Json;
using Npgsql;

namespace OpenMediaServer.Extensions;

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
            yield return JsonSerializer.Deserialize<T>(reader["json_data"].ToString() ?? string.Empty, Globals.JsonOptions);
        }
    }
}