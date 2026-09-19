using BookingValidationExceptionUnderTest =
    GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Exceptions.BookingValidationException;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Exceptions.BookingValidationException;

/// <summary>
/// Covers the constructor of <c>BookingValidationException</c>.
/// </summary>
public sealed class ConstructorTests
{
    private readonly Guid _bookingId = Guid.CreateVersion7();

    [Fact]
    public void KeepsTheBookingIdAndTheValidationErrors()
    {
        List<string> validationErrors = ["First error.", "Second error."];

        BookingValidationExceptionUnderTest exception = new(_bookingId, validationErrors);

        Assert.Equal(_bookingId, exception.BookingId);
        Assert.Same(validationErrors, exception.ValidationErrors);
    }

    [Fact]
    public void NamesTheBookingIdAndAllValidationErrorsInTheMessage()
    {
        BookingValidationExceptionUnderTest exception = new(_bookingId, ["First error.", "Second error."]);

        Assert.Equal($"Booking '{_bookingId}' is invalid: First error. Second error.", exception.Message);
    }
}
