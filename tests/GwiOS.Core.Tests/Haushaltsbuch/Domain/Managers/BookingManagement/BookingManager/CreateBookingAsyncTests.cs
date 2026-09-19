using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.BookingManagement.BookingManager;

/// <summary>
/// Covers <c>BookingManager.CreateBookingAsync</c>.
/// </summary>
public sealed class CreateBookingAsyncTests
{
    private readonly BookingManagerProbe _probe = new();
    private readonly Booking _booking = new()
    {
        Date = new DateOnly(2026, 9, 1), Type = CategoryType.Expense, CategoryId = Guid.CreateVersion7(),
        Amount = 58m, Status = BookingStatus.Booked
    };

    [Fact]
    public async Task StoresTheBooking_WhenItIsValid()
    {
        await _probe.BookingManager.CreateBookingAsync(_booking, TestContext.Current.CancellationToken);

        List<Booking> stored = await _probe.BookingRepository.GetInRangeAsync(
            DateOnly.MinValue,
            DateOnly.MaxValue,
            TestContext.Current.CancellationToken);
        Assert.Same(_booking, Assert.Single(stored));
    }

    [Fact]
    public async Task ReturnsTheStoredBooking()
    {
        Booking created = await _probe.BookingManager.CreateBookingAsync(_booking, TestContext.Current.CancellationToken);

        Assert.Same(_booking, created);
    }

    [Fact]
    public async Task ValidatesTheBooking()
    {
        await _probe.BookingManager.CreateBookingAsync(_booking, TestContext.Current.CancellationToken);

        Assert.Same(_booking, Assert.Single(_probe.BookingValidator.ValidatedBookings));
    }

    [Fact]
    public async Task ThrowsBookingValidationExceptionWithTheValidationErrors_WhenTheBookingIsInvalid()
    {
        _probe.BookingValidator.ValidationErrors = ["First error.", "Second error."];

        BookingValidationException exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _probe.BookingManager.CreateBookingAsync(_booking, TestContext.Current.CancellationToken));

        Assert.Equal(_booking.Id, exception.BookingId);
        Assert.Equal(["First error.", "Second error."], exception.ValidationErrors);
    }

    [Fact]
    public async Task DoesNotStoreTheBooking_WhenItIsInvalid()
    {
        _probe.BookingValidator.ValidationErrors = ["Error."];

        await Assert.ThrowsAsync<BookingValidationException>(
            () => _probe.BookingManager.CreateBookingAsync(_booking, TestContext.Current.CancellationToken));

        Assert.Empty(await _probe.BookingRepository.GetInRangeAsync(
            DateOnly.MinValue,
            DateOnly.MaxValue,
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LogsTheCreationWithTheBookingId()
    {
        await _probe.BookingManager.CreateBookingAsync(_booking, TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Booking created.", logEntry.Message);
        Assert.Equal(_booking.Id.ToString(), logEntry.ContextData["BookingId"]);
    }
}
