using BookingNotFoundExceptionUnderTest =
    GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Exceptions.BookingNotFoundException;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Exceptions.BookingNotFoundException;

/// <summary>
/// Covers the constructor of <c>BookingNotFoundException</c>.
/// </summary>
public sealed class ConstructorTests
{
    private readonly Guid _bookingId = Guid.CreateVersion7();

    [Fact]
    public void KeepsTheBookingId()
    {
        BookingNotFoundExceptionUnderTest exception = new(_bookingId);

        Assert.Equal(_bookingId, exception.BookingId);
    }

    [Fact]
    public void NamesTheBookingIdInTheMessage()
    {
        BookingNotFoundExceptionUnderTest exception = new(_bookingId);

        Assert.Equal($"No booking with ID '{_bookingId}' is stored.", exception.Message);
    }
}
