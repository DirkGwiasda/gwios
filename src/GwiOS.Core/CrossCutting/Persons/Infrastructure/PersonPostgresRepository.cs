using System.Data;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;
using Npgsql;
using NpgsqlTypes;

namespace GwiOS.Core.CrossCutting.Persons.Infrastructure;

/// <summary>
/// Stores persons in the table <c>persons.persons</c> of the GwiOS PostgreSQL database, using plain SQL. Unique
/// indexes keep the ID, name, short name and identity user ID of the persons unique.
/// </summary>
internal sealed class PersonPostgresRepository(NpgsqlDataSource dataSource, ILogger<PersonPostgresRepository> logger)
    : IPersonRepository
{
    private const string PrimaryKeyName = "pk_persons";
    private const string NameIndexName = "ux_persons_name";
    private const string ShortNameIndexName = "ux_persons_short_name";
    private const string IdentityUserIdIndexName = "ux_persons_identity_user_id";

    // A unique index allows any number of NULL values, so any number of persons can be without a user account.
    private const string CreateTablesSql = $"""
        CREATE SCHEMA IF NOT EXISTS persons;

        CREATE TABLE IF NOT EXISTS persons.persons
        (
            id               uuid NOT NULL,
            name             text NOT NULL,
            short_name       text NOT NULL,
            identity_user_id text NULL,
            CONSTRAINT {PrimaryKeyName} PRIMARY KEY (id)
        );

        CREATE UNIQUE INDEX IF NOT EXISTS {NameIndexName} ON persons.persons (name);
        CREATE UNIQUE INDEX IF NOT EXISTS {ShortNameIndexName} ON persons.persons (short_name);
        CREATE UNIQUE INDEX IF NOT EXISTS {IdentityUserIdIndexName} ON persons.persons (identity_user_id);
        """;

    private const string InsertSql = """
        INSERT INTO persons.persons (id, name, short_name, identity_user_id)
        VALUES (@id, @name, @short_name, @identity_user_id)
        """;

    private const string UpdateSql = """
        UPDATE persons.persons
        SET name = @name, short_name = @short_name, identity_user_id = @identity_user_id
        WHERE id = @id
        """;

    private const string SelectAllSql = """
        SELECT id, name, short_name, identity_user_id
        FROM persons.persons
        ORDER BY name
        """;

    private const string SelectByIdentityUserIdSql = """
        SELECT id, name, short_name, identity_user_id
        FROM persons.persons
        WHERE identity_user_id = @identity_user_id
        """;

    private const string DeleteSql = """
        DELETE FROM persons.persons
        WHERE id = @id
        """;

    private readonly NpgsqlDataSource _dataSource = dataSource;
    private readonly ILogger<PersonPostgresRepository> _logger = logger;

    public async Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(CreateTablesSql);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task InsertAsync(Person person, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(InsertSql);
        AddPersonParameters(command, person);
        await ExecuteWriteAsync(command, person, cancellationToken);
    }

    public async Task UpdateAsync(Person person, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(UpdateSql);
        AddPersonParameters(command, person);
        int updatedRowCount = await ExecuteWriteAsync(command, person, cancellationToken);
        if (updatedRowCount == 0)
        {
            _logger.LogWarning("Person to update is not stored.", CreatePersonIdData(person.Id));
            throw new PersonNotFoundException(person.Id);
        }
    }

    public async Task<List<Person>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(SelectAllSql);
        return await ReadPersonsAsync(command, cancellationToken);
    }

    public async Task<Person?> GetByIdentityUserIdAsync(
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(SelectByIdentityUserIdSql);
        command.Parameters.AddWithValue("identity_user_id", identityUserId);
        List<Person> persons = await ReadPersonsAsync(command, cancellationToken);
        // The unique index on identity_user_id allows at most one match.
        return persons.SingleOrDefault();
    }

    public async Task DeleteAsync(Guid personId, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(DeleteSql);
        command.Parameters.AddWithValue("id", personId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    // Uniqueness is enforced by the database only, so a conflict becomes known when the person is written.
    private async Task<int> ExecuteWriteAsync(NpgsqlCommand command, Person person, CancellationToken cancellationToken)
    {
        try
        {
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            DuplicatePersonException duplicatePersonException =
                new(person.Id, GetDuplicatePropertyName(exception), exception);
            _logger.LogWarning(duplicatePersonException.Message, CreatePersonIdData(person.Id));
            throw duplicatePersonException;
        }
    }

    private static string GetDuplicatePropertyName(PostgresException exception)
        => exception.ConstraintName switch
        {
            PrimaryKeyName => nameof(Person.Id),
            NameIndexName => nameof(Person.Name),
            ShortNameIndexName => nameof(Person.ShortName),
            IdentityUserIdIndexName => nameof(Person.IdentityUserId),
            _ => throw new InvalidOperationException(
                $"The unique constraint '{exception.ConstraintName}' of persons.persons is not known.", exception)
        };

    private static void AddPersonParameters(NpgsqlCommand command, Person person)
    {
        command.Parameters.AddWithValue("id", person.Id);
        command.Parameters.AddWithValue("name", person.Name);
        command.Parameters.AddWithValue("short_name", person.ShortName);
        command.Parameters.AddWithValue(
            "identity_user_id",
            NpgsqlDbType.Text,
            (object?)person.IdentityUserId ?? DBNull.Value);
    }

    private static async Task<List<Person>> ReadPersonsAsync(
        NpgsqlCommand command,
        CancellationToken cancellationToken)
    {
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        List<Person> persons = [];
        while (await reader.ReadAsync(cancellationToken))
        {
            persons.Add(ReadPerson(reader));
        }

        return persons;
    }

    private static Person ReadPerson(NpgsqlDataReader reader)
        => new()
        {
            Id = reader.GetGuid("id"),
            Name = reader.GetString("name"),
            ShortName = reader.GetString("short_name"),
            IdentityUserId = reader.IsDBNull("identity_user_id") ? null : reader.GetString("identity_user_id")
        };

    private static Dictionary<string, string> CreatePersonIdData(Guid personId)
        => new() { ["PersonId"] = personId.ToString() };
}
