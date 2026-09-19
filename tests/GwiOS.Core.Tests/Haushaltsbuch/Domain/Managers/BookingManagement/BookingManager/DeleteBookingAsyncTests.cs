using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.BookingManagement.BookingManager;

/// <summary>
/// Covers <c>BookingManager.DeleteBookingAsync</c>.
/// </summary>
public sealed class DeleteBookingAsyncTests
{
    private readonly BookingManagerProbe _probe = new();
    private readonly Booking _wocheneinkauf = new()
    {
        Date = new DateOnly(2026, 9, 1), Type = CategoryType.Expense, CategoryId = Guid.CreateVersion7(),
        Amount = 58m, Status = BookingStatus.Booked
    };
    private readonly Booking _gehalt = new()
    {
        Date = new DateOnly(2026, 9, 1), Type = CategoryType.Income, CategoryId = Guid.CreateVersion7(),
        Amount = 3200m, Status = BookingStatus.Booked
    };

    [Fact]
    public async Task DeletesOnlyTheBookingWithTheGivenId()
    {
        await _probe.BookingRepository.InsertAsync(_wocheneinkauf, TestContext.Current.CancellationToken);
        await _probe.BookingRepository.InsertAsync(_gehalt, TestContext.Current.CancellationToken);

        await _probe.BookingManager.DeleteBookingAsync(_wocheneinkauf.Id, TestContext.Current.CancellationToken);

        List<Booking> stored = await _probe.BookingRepository.GetInRangeAsync(
            DateOnly.MinValue,
            DateOnly.MaxValue,
            TestContext.Current.CancellationToken);
        Assert.Same(_gehalt, Assert.Single(stored));
    }

    [Fact]
    public async Task Succeeds_WhenTheBookingIsNotStored()
    {
        Exception? exception = await Record.ExceptionAsync(
            () => _probe.BookingManager.DeleteBookingAsync(_wocheneinkauf.Id, TestContext.Current.CancellationToken));

        Assert.Null(exception);
    }

    [Fact]
    public async Task LogsTheDeletionWithTheBookingId()
    {
        await _probe.BookingRepository.InsertAsync(_wocheneinkauf, TestContext.Current.CancellationToken);

        await _probe.BookingManager.DeleteBookingAsync(_wocheneinkauf.Id, TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Booking deleted.", logEntry.Message);
        Assert.Equal(_wocheneinkauf.Id.ToString(), logEntry.ContextData["BookingId"]);
    }
}
