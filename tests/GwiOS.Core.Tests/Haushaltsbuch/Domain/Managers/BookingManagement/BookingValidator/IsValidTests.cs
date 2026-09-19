using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using BookingValidatorUnderTest = GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.BookingValidator;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.BookingManagement.BookingValidator;

/// <summary>
/// Covers <c>BookingValidator.IsValid</c>.
/// </summary>
public sealed class IsValidTests
{
    private readonly LoggerFake<BookingValidatorUnderTest> _logger = new();
    private readonly BookingValidatorUnderTest _bookingValidator;

    public IsValidTests()
    {
        _bookingValidator = new BookingValidatorUnderTest(_logger);
    }

    [Fact]
    public void ReturnsTrueWithoutValidationErrors_WhenTheBookingIsValid()
    {
        Booking booking = CreateValidBooking();

        bool isValid = _bookingValidator.IsValid(booking, out List<string> validationErrors);

        Assert.True(isValid);
        Assert.Empty(validationErrors);
    }

    [Fact]
    public void ReturnsFalseWithAnError_WhenTheCategoryIdIsEmpty()
    {
        Booking booking = CreateValidBooking();
        booking.CategoryId = Guid.Empty;

        bool isValid = _bookingValidator.IsValid(booking, out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Contains("The category is required.", validationErrors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void ReturnsFalseWithAnError_WhenTheAmountIsNotGreaterThanZero(decimal amount)
    {
        Booking booking = CreateValidBooking();
        booking.Amount = amount;

        bool isValid = _bookingValidator.IsValid(booking, out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Contains("The amount must be greater than 0.", validationErrors);
    }

    [Fact]
    public void LogsAWarningWithTheBookingIdAndTheErrors_WhenTheBookingIsInvalid()
    {
        Booking booking = CreateValidBooking();
        booking.CategoryId = Guid.Empty;

        _bookingValidator.IsValid(booking, out _);

        LogEntry logEntry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Warning, logEntry.LogLevel);
        Assert.Equal(booking.Id.ToString(), logEntry.ContextData["BookingId"]);
        Assert.Contains("The category is required.", logEntry.ContextData["ValidationErrors"]);
    }

    [Fact]
    public void LogsNothing_WhenTheBookingIsValid()
    {
        Booking booking = CreateValidBooking();

        _bookingValidator.IsValid(booking, out _);

        Assert.Empty(_logger.Entries);
    }

    private static Booking CreateValidBooking()
        => new()
        {
            Date = new DateOnly(2026, 9, 1),
            Type = CategoryType.Expense,
            CategoryId = Guid.CreateVersion7(),
            Amount = 58m,
            Status = BookingStatus.Booked
        };
}
