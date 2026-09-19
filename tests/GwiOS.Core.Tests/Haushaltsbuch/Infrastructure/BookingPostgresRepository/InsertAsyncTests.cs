using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.BookingPostgresRepository;

/// <summary>
/// Covers <c>BookingPostgresRepository.InsertAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class InsertAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly IBookingRepository _bookingRepository = database.BookingRepository;
    private readonly TestBookings _testBookings = new(database.CategoryRepository, database.BookingRepository);

    [Fact]
    public async Task StoresAllPropertiesOfTheBooking()
    {
        Booking booking = await _testBookings.CreateAsync(
            new DateOnly(2026, 9, 1),
            TestContext.Current.CancellationToken);
        booking.Description = "Wocheneinkauf";
        booking.Amount = 72.5m;
        booking.Status = BookingStatus.Pending;

        await _bookingRepository.InsertAsync(booking, TestContext.Current.CancellationToken);

        Booking? stored = await _testBookings.FindStoredAsync(booking.Id);
        Assert.NotNull(stored);
        Assert.Equal(booking.Date, stored.Date);
        Assert.Equal(booking.Type, stored.Type);
        Assert.Equal(booking.CategoryId, stored.CategoryId);
        Assert.Equal(booking.Description, stored.Description);
        Assert.Equal(booking.Amount, stored.Amount);
        Assert.Equal(booking.Status, stored.Status);
        Assert.Null(stored.RecurringItemId);
    }

    [Fact]
    public async Task StoresABookingWithoutOptionalValues()
    {
        Booking booking = await _testBookings.CreateAsync(
            new DateOnly(2026, 9, 1),
            TestContext.Current.CancellationToken);

        await _bookingRepository.InsertAsync(booking, TestContext.Current.CancellationToken);

        Booking? stored = await _testBookings.FindStoredAsync(booking.Id);
        Assert.NotNull(stored);
        Assert.Null(stored.Description);
        Assert.Null(stored.RecurringItemId);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testBookings.DisposeAsync();
}
