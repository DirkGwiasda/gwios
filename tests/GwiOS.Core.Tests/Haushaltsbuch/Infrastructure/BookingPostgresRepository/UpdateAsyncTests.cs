using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.BookingPostgresRepository;

/// <summary>
/// Covers <c>BookingPostgresRepository.UpdateAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class UpdateAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly PostgresTestDatabase _database = database;
    private readonly IBookingRepository _bookingRepository = database.BookingRepository;
    private readonly TestBookings _testBookings = new(database.CategoryRepository, database.BookingRepository);

    [Fact]
    public async Task OverwritesAllChangeablePropertiesOfTheStoredBooking()
    {
        Booking booking = await _testBookings.CreateAsync(
            new DateOnly(2026, 9, 1),
            TestContext.Current.CancellationToken);
        await _bookingRepository.InsertAsync(booking, TestContext.Current.CancellationToken);
        Booking changed = new()
        {
            Id = booking.Id, Date = new DateOnly(2026, 9, 5), Type = booking.Type, CategoryId = booking.CategoryId,
            Description = "Geändert", Amount = 99.9m, Status = BookingStatus.Pending
        };

        await _bookingRepository.UpdateAsync(changed, TestContext.Current.CancellationToken);

        Booking? stored = await _testBookings.FindStoredAsync(booking.Id);
        Assert.NotNull(stored);
        Assert.Equal(changed.Date, stored.Date);
        Assert.Equal(changed.Description, stored.Description);
        Assert.Equal(changed.Amount, stored.Amount);
        Assert.Equal(changed.Status, stored.Status);
    }

    [Fact]
    public async Task ThrowsBookingNotFoundException_WhenTheBookingIsNotStored()
    {
        Booking booking = await _testBookings.CreateAsync(
            new DateOnly(2026, 9, 1),
            TestContext.Current.CancellationToken);

        BookingNotFoundException exception = await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _bookingRepository.UpdateAsync(booking, TestContext.Current.CancellationToken));

        Assert.Equal(booking.Id, exception.BookingId);
    }

    [Fact]
    public async Task LogsAWarningWithTheBookingId_WhenTheBookingIsNotStored()
    {
        Booking booking = await _testBookings.CreateAsync(
            new DateOnly(2026, 9, 1),
            TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _bookingRepository.UpdateAsync(booking, TestContext.Current.CancellationToken));

        Assert.Contains(
            _database.BookingRepositoryLogger.Entries,
            logEntry => (logEntry.LogLevel == LogLevel.Warning)
                && (logEntry.ContextData.GetValueOrDefault("BookingId") == booking.Id.ToString()));
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testBookings.DisposeAsync();
}
