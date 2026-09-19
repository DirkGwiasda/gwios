using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.BookingManagement.BookingManager;

/// <summary>
/// Covers <c>BookingManager.GetBookingsInRangeAsync</c>.
/// </summary>
public sealed class GetBookingsInRangeAsyncTests
{
    private readonly BookingManagerProbe _probe = new();

    [Fact]
    public async Task ReturnsOnlyBookingsWithinTheGivenRangeNewestFirst()
    {
        Booking beforeRange = CreateBooking(new DateOnly(2026, 8, 31));
        Booking startOfRange = CreateBooking(new DateOnly(2026, 9, 1));
        Booking endOfRange = CreateBooking(new DateOnly(2026, 9, 30));
        Booking afterRange = CreateBooking(new DateOnly(2026, 10, 1));
        await _probe.BookingRepository.InsertAsync(beforeRange, TestContext.Current.CancellationToken);
        await _probe.BookingRepository.InsertAsync(startOfRange, TestContext.Current.CancellationToken);
        await _probe.BookingRepository.InsertAsync(endOfRange, TestContext.Current.CancellationToken);
        await _probe.BookingRepository.InsertAsync(afterRange, TestContext.Current.CancellationToken);

        List<Booking> bookings = await _probe.BookingManager.GetBookingsInRangeAsync(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 30),
            TestContext.Current.CancellationToken);

        Assert.Equal([endOfRange, startOfRange], bookings);
    }

    [Fact]
    public async Task ReturnsAnEmptyList_WhenNoBookingFallsInTheRange()
    {
        List<Booking> bookings = await _probe.BookingManager.GetBookingsInRangeAsync(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 30),
            TestContext.Current.CancellationToken);

        Assert.Empty(bookings);
    }

    private static Booking CreateBooking(DateOnly date)
        => new()
        {
            Date = date, Type = CategoryType.Expense, CategoryId = Guid.CreateVersion7(), Amount = 58m,
            Status = BookingStatus.Booked
        };
}
