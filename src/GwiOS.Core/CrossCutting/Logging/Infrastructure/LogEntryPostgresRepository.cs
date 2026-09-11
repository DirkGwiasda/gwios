using System.Data;
using System.Text.Json;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using Npgsql;
using NpgsqlTypes;

namespace GwiOS.Core.CrossCutting.Logging.Infrastructure;

/// <summary>
/// Stores log entries in the table <c>logging.log_entries</c> of the GwiOS PostgreSQL database, using plain SQL.
/// </summary>
internal sealed class LogEntryPostgresRepository(NpgsqlDataSource dataSource) : ILogEntryRepository
{
    private const string CreateTablesSql = """
        CREATE SCHEMA IF NOT EXISTS logging;

        CREATE TABLE IF NOT EXISTS logging.log_entries
        (
            id           uuid        PRIMARY KEY,
            timestamp    timestamptz NOT NULL,
            app_name     text        NOT NULL,
            data_source  text        NOT NULL,
            log_level    smallint    NOT NULL,
            message      text        NOT NULL,
            context_data jsonb       NOT NULL
        );

        CREATE INDEX IF NOT EXISTS ix_log_entries_app_name_timestamp
            ON logging.log_entries (app_name, timestamp DESC);
        """;

    private const string InsertSql = """
        INSERT INTO logging.log_entries (id, timestamp, app_name, data_source, log_level, message, context_data)
        VALUES (@id, @timestamp, @app_name, @data_source, @log_level, @message, @context_data)
        """;

    private const string SelectAppNamesSql = """
        SELECT DISTINCT app_name
        FROM logging.log_entries
        ORDER BY app_name
        """;

    private const string SelectLogEntriesByAppNameSql = """
        SELECT id, timestamp, app_name, data_source, log_level, message, context_data
        FROM logging.log_entries
        WHERE app_name = @app_name
        ORDER BY timestamp DESC, id DESC
        """;

    private const string DeleteByAppNameSql = """
        DELETE FROM logging.log_entries
        WHERE app_name = @app_name
        """;

    private readonly NpgsqlDataSource _dataSource = dataSource;

    public async Task EnsureStorageCreatedAsync()
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(CreateTablesSql);
        await command.ExecuteNonQueryAsync();
    }

    public async Task InsertAsync(LogEntry logEntry)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(InsertSql);
        AddLogEntryParameters(command, logEntry);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<string>> GetAllAppNamesAsync()
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(SelectAppNamesSql);
        return await ReadAllAsync(command, reader => reader.GetString("app_name"));
    }

    public async Task<List<LogEntry>> GetAllLogEntriesByAppAsync(string appName)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(SelectLogEntriesByAppNameSql);
        command.Parameters.AddWithValue("app_name", appName);
        return await ReadAllAsync(command, ReadLogEntry);
    }

    public async Task DeleteByAppNameAsync(string appName)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(DeleteByAppNameSql);
        command.Parameters.AddWithValue("app_name", appName);
        await command.ExecuteNonQueryAsync();
    }

    private static void AddLogEntryParameters(NpgsqlCommand command, LogEntry logEntry)
    {
        command.Parameters.AddWithValue("id", logEntry.Id);
        // Npgsql only writes timestamptz values with a zero offset.
        command.Parameters.AddWithValue("timestamp", logEntry.Timestamp.ToUniversalTime());
        command.Parameters.AddWithValue("app_name", logEntry.AppName);
        command.Parameters.AddWithValue("data_source", logEntry.DataSource);
        command.Parameters.AddWithValue("log_level", (short)logEntry.LogLevel);
        command.Parameters.AddWithValue("message", logEntry.Message);
        command.Parameters.AddWithValue("context_data", NpgsqlDbType.Jsonb, JsonSerializer.Serialize(logEntry.ContextData));
    }

    private static async Task<List<T>> ReadAllAsync<T>(NpgsqlCommand command, Func<NpgsqlDataReader, T> readRow)
    {
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        List<T> rows = [];
        while (await reader.ReadAsync())
        {
            rows.Add(readRow(reader));
        }

        return rows;
    }

    private static LogEntry ReadLogEntry(NpgsqlDataReader reader)
        => new(
            reader.GetString("app_name"),
            reader.GetString("data_source"),
            reader.GetString("message"),
            (LogLevel)reader.GetInt16("log_level"),
            ReadContextData(reader))
        {
            Id = reader.GetGuid("id"),
            Timestamp = reader.GetFieldValue<DateTimeOffset>("timestamp")
        };

    private static Dictionary<string, string> ReadContextData(NpgsqlDataReader reader)
        => JsonSerializer.Deserialize<Dictionary<string, string>>(reader.GetString("context_data")) ?? [];
}
