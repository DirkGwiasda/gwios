using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringBookingGeneration.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Workflows.RecurringBookingGeneration.RecurringBookingGenerationWorkflow;

/// <summary>
/// Covers <c>RecurringBookingGenerationWorkflow.ConfirmAsync</c>.
/// </summary>
public sealed class ConfirmAsyncTests
{
    private readonly RecurringBookingGenerationWorkflowProbe _probe = new();
    private readonly ProposedBooking _proposed = new()
    {
        RecurringItemId = Guid.CreateVersion7(), Name = "Miete", Type = CategoryType.Expense,
        CategoryId = Guid.CreateVersion7(), Date = new DateOnly(2026, 9, 5), Amount = 1150m
    };

    [Fact]
    public async Task CreatesAPendingBookingForEachSelectedProposal()
    {
        await _probe.Workflow.ConfirmAsync(
            new DateOnly(2026, 9, 1),
            [_proposed],
            TestContext.Current.CancellationToken);

        Booking created = Assert.Single(_probe.BookingManager.CreatedBookings);
        Assert.Equal(_proposed.Date, created.Date);
        Assert.Equal(_proposed.Type, created.Type);
        Assert.Equal(_proposed.CategoryId, created.CategoryId);
        Assert.Equal(_proposed.Name, created.Description);
        Assert.Equal(_proposed.Amount, created.Amount);
        Assert.Equal(BookingStatus.Pending, created.Status);
        Assert.Equal(_proposed.RecurringItemId, created.RecurringItemId);
    }

    [Fact]
    public async Task MarksTheMonthAsReviewed_EvenWithoutSelectedProposals()
    {
        await _probe.Workflow.ConfirmAsync(
            new DateOnly(2026, 9, 15),
            [],
            TestContext.Current.CancellationToken);

        Assert.Equal([new DateOnly(2026, 9, 1)], _probe.MonthReviewManager.MarkedReviewedMonths);
        Assert.Empty(_probe.BookingManager.CreatedBookings);
    }

    [Fact]
    public async Task LogsTheConfirmationWithTheMonthAndCount()
    {
        await _probe.Workflow.ConfirmAsync(
            new DateOnly(2026, 9, 1),
            [_proposed],
            TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Recurring bookings confirmed for month.", logEntry.Message);
        Assert.Equal("1", logEntry.ContextData["ConfirmedCount"]);
    }
}
