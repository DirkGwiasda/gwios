using System.Data;
using System.Text.Json;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NpgsqlTypes;

namespace GwiOS.Core.CrossCutting.Logging.Infrastructure;

/// <summary>
/// Stores log entries in the table <c>logging.log_entries</c> of the GwiOS PostgreSQL database, using plain SQL.
/// </summary>
internal sealed class LogEntryPostgresRepository(
    NpgsqlDataSource dataSource,
    [FromKeyedServices(LogEntryPostgresRepository.MaintenanceDataSourceKey)] NpgsqlDataSource maintenanceDataSource)
    : ILogEntryRepository
{
    /// <summary>
    /// Service key of the data source that connects to the server's maintenance database instead of the GwiOS
    /// database. It is needed to create the GwiOS database, which cannot be connected to before it exists.
    /// </summary>
    internal const string MaintenanceDataSourceKey = "GwiOS.Postgres.Maintenance";

    private const string DatabaseExistsSql = """
        SELECT EXISTS (SELECT 1 FROM pg_database WHERE datname = @database_name)
        """;

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
    private readonly NpgsqlDataSource _maintenanceDataSource = maintenanceDataSource;

    public async Task EnsureStorageCreatedAsync()
    {
        await EnsureDatabaseCreatedAsync();
        await EnsureTablesCreatedAsync();
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

    private async Task EnsureDatabaseCreatedAsync()
    {
        string databaseName = GetDatabaseName();
        if (await DatabaseExistsAsync(databaseName))
        {
            return;
        }

        await CreateDatabaseUnlessCreatedConcurrentlyAsync(databaseName);
    }

    private string GetDatabaseName()
        => new NpgsqlConnectionStringBuilder(_dataSource.ConnectionString).Database
            ?? throw new InvalidOperationException(
                "The connection string of the GwiOS data source does not specify a database (Database=...).");

    private async Task<bool> DatabaseExistsAsync(string databaseName)
    {
        await using NpgsqlCommand command = _maintenanceDataSource.CreateCommand(DatabaseExistsSql);
        command.Parameters.AddWithValue("database_name", databaseName);
        return (bool)(await command.ExecuteScalarAsync())!;
    }

    private async Task CreateDatabaseUnlessCreatedConcurrentlyAsync(string databaseName)
    {
        try
        {
            await CreateDatabaseAsync(databaseName);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.DuplicateDatabase)
        {
            // Another instance created the database between the existence check and this call - the desired state.
        }
    }

    private async Task CreateDatabaseAsync(string databaseName)
    {
        // CREATE DATABASE accepts no parameters, so the name has to be embedded as a quoted identifier.
        await using NpgsqlCommand command =
            _maintenanceDataSource.CreateCommand($"CREATE DATABASE {QuoteIdentifier(databaseName)}");
        await command.ExecuteNonQueryAsync();
    }

    private static string QuoteIdentifier(string identifier)
        => $"\"{identifier.Replace("\"", "\"\"")}\"";

    private async Task EnsureTablesCreatedAsync()
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(CreateTablesSql);
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
