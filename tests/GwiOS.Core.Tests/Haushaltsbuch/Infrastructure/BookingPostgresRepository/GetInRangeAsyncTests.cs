using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.BookingPostgresRepository;

/// <summary>
/// Covers <c>BookingPostgresRepository.GetInRangeAsync</c> against the PostgreSQL test database. The database may
/// contain bookings of other tests, so the tests only look at the bookings they created themselves.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class GetInRangeAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly IBookingRepository _bookingRepository = database.BookingRepository;
    private readonly TestBookings _testBookings = new(database.CategoryRepository, database.BookingRepository);

    [Fact]
    public async Task ReturnsOnlyBookingsWithinTheGivenRange()
    {
        Booking inRange = await _testBookings.CreateAsync(
            new DateOnly(2026, 9, 15),
            TestContext.Current.CancellationToken);
        Booking outOfRange = await _testBookings.CreateAsync(
            new DateOnly(2026, 10, 15),
            TestContext.Current.CancellationToken);
        await _bookingRepository.InsertAsync(inRange, TestContext.Current.CancellationToken);
        await _bookingRepository.InsertAsync(outOfRange, TestContext.Current.CancellationToken);

        List<Booking> bookings = await _bookingRepository.GetInRangeAsync(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 30),
            TestContext.Current.CancellationToken);

        Assert.Contains(bookings, booking => booking.Id == inRange.Id);
        Assert.DoesNotContain(bookings, booking => booking.Id == outOfRange.Id);
    }

    [Fact]
    public async Task ReturnsTheBookingsNewestFirst()
    {
        Booking earlier = await _testBookings.CreateAsync(
            new DateOnly(2026, 9, 5),
            TestContext.Current.CancellationToken);
        Booking later = await _testBookings.CreateAsync(
            new DateOnly(2026, 9, 20),
            TestContext.Current.CancellationToken);
        await _bookingRepository.InsertAsync(earlier, TestContext.Current.CancellationToken);
        await _bookingRepository.InsertAsync(later, TestContext.Current.CancellationToken);

        List<Booking> bookings = await _bookingRepository.GetInRangeAsync(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 30),
            TestContext.Current.CancellationToken);

        List<Guid> orderedIds = [.. bookings
            .Where(booking => (booking.Id == earlier.Id) || (booking.Id == later.Id))
            .Select(booking => booking.Id)];
        Assert.Equal([later.Id, earlier.Id], orderedIds);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testBookings.DisposeAsync();
}
