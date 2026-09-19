using System.Data;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using Npgsql;

namespace GwiOS.Core.Haushaltsbuch.Infrastructure;

/// <summary>
/// Stores Haushaltsbuch recurring items in the table <c>haushaltsbuch.recurring_items</c> of the GwiOS PostgreSQL
/// database, using plain SQL.
/// </summary>
internal sealed class RecurringItemPostgresRepository(
    NpgsqlDataSource dataSource,
    ILogger<RecurringItemPostgresRepository> logger) : IRecurringItemRepository
{
    private const string CreateTablesSql = """
        CREATE SCHEMA IF NOT EXISTS haushaltsbuch;

        CREATE TABLE IF NOT EXISTS haushaltsbuch.recurring_items
        (
            id          uuid          NOT NULL,
            name        text          NOT NULL,
            type        smallint      NOT NULL,
            category_id uuid          NOT NULL,
            amount      numeric(10,2) NOT NULL,
            interval    smallint      NOT NULL,
            due_day     integer       NOT NULL,
            start_month integer       NOT NULL,
            active      boolean       NOT NULL,
            CONSTRAINT pk_haushaltsbuch_recurring_items PRIMARY KEY (id),
            CONSTRAINT fk_haushaltsbuch_recurring_items_category FOREIGN KEY (category_id)
                REFERENCES haushaltsbuch.categories (id)
        );
        """;

    private const string InsertSql = """
        INSERT INTO haushaltsbuch.recurring_items
            (id, name, type, category_id, amount, interval, due_day, start_month, active)
        VALUES (@id, @name, @type, @category_id, @amount, @interval, @due_day, @start_month, @active)
        """;

    private const string UpdateSql = """
        UPDATE haushaltsbuch.recurring_items
        SET name = @name, type = @type, category_id = @category_id, amount = @amount, interval = @interval,
            due_day = @due_day, start_month = @start_month, active = @active
        WHERE id = @id
        """;

    private const string SelectAllSql = """
        SELECT id, name, type, category_id, amount, interval, due_day, start_month, active
        FROM haushaltsbuch.recurring_items
        ORDER BY type, name
        """;

    private const string DeleteSql = """
        DELETE FROM haushaltsbuch.recurring_items
        WHERE id = @id
        """;

    private readonly NpgsqlDataSource _dataSource = dataSource;
    private readonly ILogger<RecurringItemPostgresRepository> _logger = logger;

    public async Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(CreateTablesSql);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task InsertAsync(RecurringItem recurringItem, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(InsertSql);
        AddRecurringItemParameters(command, recurringItem);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(RecurringItem recurringItem, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(UpdateSql);
        AddRecurringItemParameters(command, recurringItem);
        int updatedRowCount = await command.ExecuteNonQueryAsync(cancellationToken);
        if (updatedRowCount == 0)
        {
            _logger.LogWarning("Recurring item to update is not stored.", CreateRecurringItemIdData(recurringItem.Id));
            throw new RecurringItemNotFoundException(recurringItem.Id);
        }
    }

    public async Task<List<RecurringItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(SelectAllSql);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        List<RecurringItem> recurringItems = [];
        while (await reader.ReadAsync(cancellationToken))
        {
            recurringItems.Add(ReadRecurringItem(reader));
        }

        return recurringItems;
    }

    public async Task DeleteAsync(Guid recurringItemId, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(DeleteSql);
        command.Parameters.AddWithValue("id", recurringItemId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddRecurringItemParameters(NpgsqlCommand command, RecurringItem recurringItem)
    {
        command.Parameters.AddWithValue("id", recurringItem.Id);
        command.Parameters.AddWithValue("name", recurringItem.Name);
        command.Parameters.AddWithValue("type", (short)recurringItem.Type);
        command.Parameters.AddWithValue("category_id", recurringItem.CategoryId);
        command.Parameters.AddWithValue("amount", recurringItem.Amount);
        command.Parameters.AddWithValue("interval", (short)recurringItem.Interval);
        command.Parameters.AddWithValue("due_day", recurringItem.DueDay);
        command.Parameters.AddWithValue("start_month", recurringItem.StartMonth);
        command.Parameters.AddWithValue("active", recurringItem.Active);
    }

    private static RecurringItem ReadRecurringItem(NpgsqlDataReader reader)
        => new()
        {
            Id = reader.GetGuid("id"),
            Name = reader.GetString("name"),
            Type = (CategoryType)reader.GetFieldValue<short>("type"),
            CategoryId = reader.GetGuid("category_id"),
            Amount = reader.GetDecimal("amount"),
            Interval = (RecurringInterval)reader.GetFieldValue<short>("interval"),
            DueDay = reader.GetInt32("due_day"),
            StartMonth = reader.GetInt32("start_month"),
            Active = reader.GetBoolean("active")
        };

    private static Dictionary<string, string> CreateRecurringItemIdData(Guid recurringItemId)
        => new() { ["RecurringItemId"] = recurringItemId.ToString() };
}
