using System.Data;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using Npgsql;
using NpgsqlTypes;

namespace GwiOS.Core.Haushaltsbuch.Infrastructure;

/// <summary>
/// Stores Haushaltsbuch bookings in the table <c>haushaltsbuch.bookings</c> of the GwiOS PostgreSQL database, using
/// plain SQL.
/// </summary>
internal sealed class BookingPostgresRepository(NpgsqlDataSource dataSource, ILogger<BookingPostgresRepository> logger)
    : IBookingRepository
{
    // A recurring item may be deleted freely (no guard); its bookings survive and simply lose the backlink.
    private const string CreateTablesSql = """
        CREATE SCHEMA IF NOT EXISTS haushaltsbuch;

        CREATE TABLE IF NOT EXISTS haushaltsbuch.bookings
        (
            id                uuid          NOT NULL,
            date              date          NOT NULL,
            type              smallint      NOT NULL,
            category_id       uuid          NOT NULL,
            description       text          NULL,
            amount            numeric(10,2) NOT NULL,
            status            smallint      NOT NULL,
            recurring_item_id uuid          NULL,
            CONSTRAINT pk_haushaltsbuch_bookings PRIMARY KEY (id),
            CONSTRAINT fk_haushaltsbuch_bookings_category FOREIGN KEY (category_id)
                REFERENCES haushaltsbuch.categories (id),
            CONSTRAINT fk_haushaltsbuch_bookings_recurring_item FOREIGN KEY (recurring_item_id)
                REFERENCES haushaltsbuch.recurring_items (id) ON DELETE SET NULL
        );
        """;

    private const string InsertSql = """
        INSERT INTO haushaltsbuch.bookings
            (id, date, type, category_id, description, amount, status, recurring_item_id)
        VALUES (@id, @date, @type, @category_id, @description, @amount, @status, @recurring_item_id)
        """;

    private const string UpdateSql = """
        UPDATE haushaltsbuch.bookings
        SET date = @date, type = @type, category_id = @category_id, description = @description, amount = @amount,
            status = @status, recurring_item_id = @recurring_item_id
        WHERE id = @id
        """;

    private const string SelectInRangeSql = """
        SELECT id, date, type, category_id, description, amount, status, recurring_item_id
        FROM haushaltsbuch.bookings
        WHERE date BETWEEN @from_date AND @to_date
        ORDER BY date DESC, id
        """;

    private const string DeleteSql = """
        DELETE FROM haushaltsbuch.bookings
        WHERE id = @id
        """;

    private readonly NpgsqlDataSource _dataSource = dataSource;
    private readonly ILogger<BookingPostgresRepository> _logger = logger;

    public async Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(CreateTablesSql);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task InsertAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(InsertSql);
        AddBookingParameters(command, booking);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(UpdateSql);
        AddBookingParameters(command, booking);
        int updatedRowCount = await command.ExecuteNonQueryAsync(cancellationToken);
        if (updatedRowCount == 0)
        {
            _logger.LogWarning("Booking to update is not stored.", CreateBookingIdData(booking.Id));
            throw new BookingNotFoundException(booking.Id);
        }
    }

    public async Task<List<Booking>> GetInRangeAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(SelectInRangeSql);
        command.Parameters.AddWithValue("from_date", NpgsqlDbType.Date, fromDate);
        command.Parameters.AddWithValue("to_date", NpgsqlDbType.Date, toDate);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        List<Booking> bookings = [];
        while (await reader.ReadAsync(cancellationToken))
        {
            bookings.Add(ReadBooking(reader));
        }

        return bookings;
    }

    public async Task DeleteAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(DeleteSql);
        command.Parameters.AddWithValue("id", bookingId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddBookingParameters(NpgsqlCommand command, Booking booking)
    {
        command.Parameters.AddWithValue("id", booking.Id);
        command.Parameters.AddWithValue("date", NpgsqlDbType.Date, booking.Date);
        command.Parameters.AddWithValue("type", (short)booking.Type);
        command.Parameters.AddWithValue("category_id", booking.CategoryId);
        command.Parameters.AddWithValue("description", NpgsqlDbType.Text, (object?)booking.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("amount", booking.Amount);
        command.Parameters.AddWithValue("status", (short)booking.Status);
        command.Parameters.AddWithValue(
            "recurring_item_id",
            NpgsqlDbType.Uuid,
            (object?)booking.RecurringItemId ?? DBNull.Value);
    }

    private static Booking ReadBooking(NpgsqlDataReader reader)
        => new()
        {
            Id = reader.GetGuid("id"),
            Date = reader.GetFieldValue<DateOnly>("date"),
            Type = (CategoryType)reader.GetFieldValue<short>("type"),
            CategoryId = reader.GetGuid("category_id"),
            Description = reader.IsDBNull("description") ? null : reader.GetString("description"),
            Amount = reader.GetDecimal("amount"),
            Status = (BookingStatus)reader.GetFieldValue<short>("status"),
            RecurringItemId = reader.IsDBNull("recurring_item_id") ? null : reader.GetGuid("recurring_item_id")
        };

    private static Dictionary<string, string> CreateBookingIdData(Guid bookingId)
        => new() { ["BookingId"] = bookingId.ToString() };
}
