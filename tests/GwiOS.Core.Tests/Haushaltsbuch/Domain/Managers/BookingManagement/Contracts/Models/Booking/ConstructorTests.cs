using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using BookingUnderTest = GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models.Booking;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models.Booking;

/// <summary>
/// Covers the construction of <c>Booking</c> and the defaults it assigns.
/// </summary>
public sealed class ConstructorTests
{
    [Fact]
    public void AssignsAVersion7Id()
    {
        BookingUnderTest booking = CreateBooking();

        Assert.Equal(7, booking.Id.Version);
    }

    [Fact]
    public void AssignsADifferentIdToEachBooking()
    {
        BookingUnderTest first = CreateBooking();
        BookingUnderTest second = CreateBooking();

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void LeavesDescriptionAndRecurringItemIdUnset()
    {
        BookingUnderTest booking = CreateBooking();

        Assert.Null(booking.Description);
        Assert.Null(booking.RecurringItemId);
    }

    private static BookingUnderTest CreateBooking()
        => new()
        {
            Date = new DateOnly(2026, 9, 1),
            Type = CategoryType.Expense,
            CategoryId = Guid.CreateVersion7(),
            Amount = 58m,
            Status = BookingStatus.Booked
        };
}
