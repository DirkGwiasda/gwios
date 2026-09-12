using System.Data;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Exceptions;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;
using Npgsql;
using NpgsqlTypes;

namespace GwiOS.Core.ToDos.Infrastructure;

/// <summary>
/// Stores ToDos in the table <c>todos.todos</c> of the GwiOS PostgreSQL database, using plain SQL.
/// </summary>
internal sealed class ToDoPostgresRepository(NpgsqlDataSource dataSource, ILogger<ToDoPostgresRepository> logger)
    : IToDoRepository
{
    private const string CreateTablesSql = """
        CREATE SCHEMA IF NOT EXISTS todos;

        CREATE TABLE IF NOT EXISTS todos.todos
        (
            id           uuid        NOT NULL,
            title        text        NOT NULL,
            description  text        NULL,
            is_completed boolean     NOT NULL,
            due_date     date        NULL,
            created_at   timestamptz NOT NULL,
            completed_at timestamptz NULL,
            position     integer     NOT NULL,
            CONSTRAINT pk_todos PRIMARY KEY (id)
        );
        """;

    private const string InsertSql = """
        INSERT INTO todos.todos (id, title, description, is_completed, due_date, created_at, completed_at, position)
        VALUES (@id, @title, @description, @is_completed, @due_date, @created_at, @completed_at, @position)
        """;

    // The creation time never changes, so it is not overwritten.
    private const string UpdateSql = """
        UPDATE todos.todos
        SET title = @title, description = @description, is_completed = @is_completed, due_date = @due_date,
            completed_at = @completed_at, position = @position
        WHERE id = @id
        """;

    private const string SelectAllSql = """
        SELECT id, title, description, is_completed, due_date, created_at, completed_at, position
        FROM todos.todos
        ORDER BY position, created_at
        """;

    private const string DeleteSql = """
        DELETE FROM todos.todos
        WHERE id = @id
        """;

    private readonly NpgsqlDataSource _dataSource = dataSource;
    private readonly ILogger<ToDoPostgresRepository> _logger = logger;

    public async Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(CreateTablesSql);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task InsertAsync(ToDo toDo, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(InsertSql);
        AddToDoParameters(command, toDo);
        command.Parameters.AddWithValue("created_at", NpgsqlDbType.TimestampTz, toDo.CreatedAt);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(ToDo toDo, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(UpdateSql);
        AddToDoParameters(command, toDo);
        int updatedRowCount = await command.ExecuteNonQueryAsync(cancellationToken);
        if (updatedRowCount == 0)
        {
            _logger.LogWarning("ToDo to update is not stored.", CreateToDoIdData(toDo.Id));
            throw new ToDoNotFoundException(toDo.Id);
        }
    }

    public async Task<List<ToDo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(SelectAllSql);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        List<ToDo> toDos = [];
        while (await reader.ReadAsync(cancellationToken))
        {
            toDos.Add(ReadToDo(reader));
        }

        return toDos;
    }

    public async Task DeleteAsync(Guid toDoId, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(DeleteSql);
        command.Parameters.AddWithValue("id", toDoId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddToDoParameters(NpgsqlCommand command, ToDo toDo)
    {
        command.Parameters.AddWithValue("id", toDo.Id);
        command.Parameters.AddWithValue("title", toDo.Title);
        command.Parameters.AddWithValue("description", NpgsqlDbType.Text, (object?)toDo.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("is_completed", toDo.IsCompleted);
        command.Parameters.AddWithValue("due_date", NpgsqlDbType.Date, (object?)toDo.DueDate ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "completed_at",
            NpgsqlDbType.TimestampTz,
            (object?)toDo.CompletedAt ?? DBNull.Value);
        command.Parameters.AddWithValue("position", toDo.Position);
    }

    private static ToDo ReadToDo(NpgsqlDataReader reader)
        => new()
        {
            Id = reader.GetGuid("id"),
            Title = reader.GetString("title"),
            Description = reader.IsDBNull("description") ? null : reader.GetString("description"),
            IsCompleted = reader.GetBoolean("is_completed"),
            DueDate = reader.IsDBNull("due_date") ? null : reader.GetFieldValue<DateOnly>("due_date"),
            CreatedAt = reader.GetDateTime("created_at"),
            CompletedAt = reader.IsDBNull("completed_at") ? null : reader.GetDateTime("completed_at"),
            Position = reader.GetInt32("position")
        };

    private static Dictionary<string, string> CreateToDoIdData(Guid toDoId)
        => new() { ["ToDoId"] = toDoId.ToString() };
}
