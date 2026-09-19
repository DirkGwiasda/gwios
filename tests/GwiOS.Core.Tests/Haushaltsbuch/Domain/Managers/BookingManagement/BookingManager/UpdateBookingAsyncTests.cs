using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.BookingManagement.BookingManager;

/// <summary>
/// Covers <c>BookingManager.UpdateBookingAsync</c>.
/// </summary>
public sealed class UpdateBookingAsyncTests
{
    private readonly BookingManagerProbe _probe = new();
    private readonly Booking _booking = new()
    {
        Date = new DateOnly(2026, 9, 1), Type = CategoryType.Expense, CategoryId = Guid.CreateVersion7(),
        Amount = 58m, Status = BookingStatus.Booked
    };

    [Fact]
    public async Task StoresTheChangedBooking_WhenItIsValid()
    {
        await _probe.BookingRepository.InsertAsync(_booking, TestContext.Current.CancellationToken);
        Booking changed = new()
        {
            Id = _booking.Id, Date = _booking.Date, Type = _booking.Type, CategoryId = _booking.CategoryId,
            Description = "Wocheneinkauf", Amount = 72.5m, Status = BookingStatus.Pending
        };

        await _probe.BookingManager.UpdateBookingAsync(changed, TestContext.Current.CancellationToken);

        Booking stored = Assert.Single(await _probe.BookingRepository.GetInRangeAsync(
            DateOnly.MinValue,
            DateOnly.MaxValue,
            TestContext.Current.CancellationToken));
        Assert.Equal("Wocheneinkauf", stored.Description);
        Assert.Equal(72.5m, stored.Amount);
        Assert.Equal(BookingStatus.Pending, stored.Status);
    }

    [Fact]
    public async Task ValidatesTheBooking()
    {
        await _probe.BookingRepository.InsertAsync(_booking, TestContext.Current.CancellationToken);

        await _probe.BookingManager.UpdateBookingAsync(_booking, TestContext.Current.CancellationToken);

        Assert.Same(_booking, Assert.Single(_probe.BookingValidator.ValidatedBookings));
    }

    [Fact]
    public async Task ThrowsBookingValidationExceptionWithTheValidationErrors_WhenTheBookingIsInvalid()
    {
        await _probe.BookingRepository.InsertAsync(_booking, TestContext.Current.CancellationToken);
        _probe.BookingValidator.ValidationErrors = ["First error.", "Second error."];

        BookingValidationException exception = await Assert.ThrowsAsync<BookingValidationException>(
            () => _probe.BookingManager.UpdateBookingAsync(_booking, TestContext.Current.CancellationToken));

        Assert.Equal(_booking.Id, exception.BookingId);
        Assert.Equal(["First error.", "Second error."], exception.ValidationErrors);
    }

    [Fact]
    public async Task ThrowsBookingNotFoundException_WhenTheBookingIsNotStored()
    {
        await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _probe.BookingManager.UpdateBookingAsync(_booking, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LogsTheUpdateWithTheBookingId()
    {
        await _probe.BookingRepository.InsertAsync(_booking, TestContext.Current.CancellationToken);

        await _probe.BookingManager.UpdateBookingAsync(_booking, TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Booking updated.", logEntry.Message);
        Assert.Equal(_booking.Id.ToString(), logEntry.ContextData["BookingId"]);
    }
}
