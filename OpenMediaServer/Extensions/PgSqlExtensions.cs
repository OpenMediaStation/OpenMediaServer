using System.Data;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Dapper;
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
        await using var command = new NpgsqlCommand(sql, connection);
        // command.Parameters.AddRange(parameters.Select(p => new NpgsqlParameter(p.GetType().Name, p)).ToArray());
        return await command.ExecuteNonQueryAsync();
    }

    public static async IAsyncEnumerable<T> QueryAsync<T>(
        this NpgsqlConnection connection,
        string sql,
        object? parameters = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Streams rows without buffering the whole result set
        await foreach (var row in connection
                           .QueryUnbufferedAsync<T>(sql, parameters, commandType: CommandType.Text)
                           .WithCancellation(cancellationToken))
        {
            yield return row;
        }
    }
}