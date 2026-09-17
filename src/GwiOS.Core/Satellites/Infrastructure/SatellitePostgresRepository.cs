using System.Data;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using Npgsql;
using NpgsqlTypes;

namespace GwiOS.Core.Satellites.Infrastructure;

/// <summary>
/// Stores satellites in the table <c>satellites.satellites</c> of the GwiOS PostgreSQL database, using plain SQL. The
/// primary key keeps the names of the satellites unique.
/// </summary>
internal sealed class SatellitePostgresRepository(
    NpgsqlDataSource dataSource,
    ILogger<SatellitePostgresRepository> logger) : ISatelliteRepository
{
    private const string CreateTablesSql = """
        CREATE SCHEMA IF NOT EXISTS satellites;

        CREATE TABLE IF NOT EXISTS satellites.satellites
        (
            name            text        NOT NULL,
            display_name    text        NULL,
            is_locked       boolean     NOT NULL,
            protected_token text        NULL,
            created_at      timestamptz NOT NULL,
            CONSTRAINT pk_satellites PRIMARY KEY (name)
        );
        """;

    private const string InsertSql = """
        INSERT INTO satellites.satellites (name, display_name, is_locked, protected_token, created_at)
        VALUES (@name, @display_name, @is_locked, @protected_token, @created_at)
        """;

    private const string UpdateSql = """
        UPDATE satellites.satellites
        SET display_name = @display_name, is_locked = @is_locked, protected_token = @protected_token
        WHERE name = @name
        """;

    private const string SelectAllSql = """
        SELECT name, display_name, is_locked, protected_token, created_at
        FROM satellites.satellites
        ORDER BY name
        """;

    private const string SelectByNameSql = """
        SELECT name, display_name, is_locked, protected_token, created_at
        FROM satellites.satellites
        WHERE name = @name
        """;

    private const string DeleteSql = """
        DELETE FROM satellites.satellites
        WHERE name = @name
        """;

    private readonly NpgsqlDataSource _dataSource = dataSource;
    private readonly ILogger<SatellitePostgresRepository> _logger = logger;

    public async Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(CreateTablesSql);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task InsertAsync(Satellite satellite, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(InsertSql);
        AddSatelliteParameters(command, satellite);
        command.Parameters.AddWithValue("created_at", NpgsqlDbType.TimestampTz, satellite.CreatedAt);
        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            DuplicateSatelliteException duplicateSatelliteException = new(satellite.Name, exception);
            _logger.LogWarning(duplicateSatelliteException.Message, CreateSatelliteNameData(satellite.Name));
            throw duplicateSatelliteException;
        }
    }

    public async Task UpdateAsync(Satellite satellite, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(UpdateSql);
        AddSatelliteParameters(command, satellite);
        int updatedRowCount = await command.ExecuteNonQueryAsync(cancellationToken);
        if (updatedRowCount == 0)
        {
            _logger.LogWarning("Satellite to update is not stored.", CreateSatelliteNameData(satellite.Name));
            throw new SatelliteNotFoundException(satellite.Name);
        }
    }

    public async Task<List<Satellite>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(SelectAllSql);
        return await ReadSatellitesAsync(command, cancellationToken);
    }

    public async Task<Satellite?> GetByNameAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(SelectByNameSql);
        command.Parameters.AddWithValue("name", satelliteName);
        List<Satellite> satellites = await ReadSatellitesAsync(command, cancellationToken);
        // The primary key on name allows at most one match.
        return satellites.SingleOrDefault();
    }

    public async Task DeleteAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(DeleteSql);
        command.Parameters.AddWithValue("name", satelliteName);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddSatelliteParameters(NpgsqlCommand command, Satellite satellite)
    {
        command.Parameters.AddWithValue("name", satellite.Name);
        command.Parameters.AddWithValue("display_name", NpgsqlDbType.Text, (object?)satellite.DisplayName ?? DBNull.Value);
        command.Parameters.AddWithValue("is_locked", satellite.IsLocked);
        command.Parameters.AddWithValue(
            "protected_token",
            NpgsqlDbType.Text,
            (object?)satellite.ProtectedToken ?? DBNull.Value);
    }

    private static async Task<List<Satellite>> ReadSatellitesAsync(
        NpgsqlCommand command,
        CancellationToken cancellationToken)
    {
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        List<Satellite> satellites = [];
        while (await reader.ReadAsync(cancellationToken))
        {
            satellites.Add(ReadSatellite(reader));
        }

        return satellites;
    }

    private static Satellite ReadSatellite(NpgsqlDataReader reader)
        => new()
        {
            Name = reader.GetString("name"),
            DisplayName = reader.IsDBNull("display_name") ? null : reader.GetString("display_name"),
            IsLocked = reader.GetBoolean("is_locked"),
            ProtectedToken = reader.IsDBNull("protected_token") ? null : reader.GetString("protected_token"),
            CreatedAt = reader.GetDateTime("created_at")
        };

    private static Dictionary<string, string> CreateSatelliteNameData(string satelliteName)
        => new() { ["SatelliteName"] = satelliteName };
}
