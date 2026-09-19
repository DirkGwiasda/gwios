using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringBookingGeneration.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Workflows.RecurringBookingGeneration.RecurringBookingGenerationWorkflow;

/// <summary>
/// Covers <c>RecurringBookingGenerationWorkflow.GetProposedBookingsAsync</c>.
/// </summary>
public sealed class GetProposedBookingsAsyncTests
{
    private readonly RecurringBookingGenerationWorkflowProbe _probe = new();

    [Fact]
    public async Task ProposesAMonthlyItemOnItsDueDay()
    {
        RecurringItem miete = CreateItem("Miete", RecurringInterval.Monthly, dueDay: 5);
        _probe.RecurringItemManager.Add(miete);

        List<ProposedBooking> proposed = await _probe.Workflow.GetProposedBookingsAsync(
            new DateOnly(2026, 9, 1),
            TestContext.Current.CancellationToken);

        ProposedBooking booking = Assert.Single(proposed);
        Assert.Equal(miete.Id, booking.RecurringItemId);
        Assert.Equal(new DateOnly(2026, 9, 5), booking.Date);
        Assert.Equal(miete.Amount, booking.Amount);
    }

    [Fact]
    public async Task ProposesAWeeklyItemFourTimes()
    {
        _probe.RecurringItemManager.Add(CreateItem("Wocheneinkauf", RecurringInterval.Weekly));

        List<ProposedBooking> proposed = await _probe.Workflow.GetProposedBookingsAsync(
            new DateOnly(2026, 9, 1),
            TestContext.Current.CancellationToken);

        Assert.Equal(
            [new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 8), new DateOnly(2026, 9, 15), new DateOnly(2026, 9, 22)],
            proposed.Select(booking => booking.Date));
    }

    [Fact]
    public async Task ClampsTheDueDayToTheLastDayOfAShorterMonth()
    {
        _probe.RecurringItemManager.Add(CreateItem("Miete", RecurringInterval.Monthly, dueDay: 31));

        List<ProposedBooking> proposed = await _probe.Workflow.GetProposedBookingsAsync(
            new DateOnly(2026, 2, 1),
            TestContext.Current.CancellationToken);

        ProposedBooking booking = Assert.Single(proposed);
        Assert.Equal(new DateOnly(2026, 2, 28), booking.Date);
    }

    [Theory]
    [InlineData(0, 7, true)] // due every 3rd month starting at month index 0 (January): Jan, Apr, Jul, Oct
    [InlineData(0, 8, false)]
    [InlineData(0, 10, true)]
    public async Task ProposesAQuarterlyItemOnlyInMatchingMonths(int startMonth, int month, bool expectedDue)
    {
        _probe.RecurringItemManager.Add(CreateItem("Rundfunk", RecurringInterval.Quarterly, startMonth: startMonth));

        List<ProposedBooking> proposed = await _probe.Workflow.GetProposedBookingsAsync(
            new DateOnly(2026, month, 1),
            TestContext.Current.CancellationToken);

        Assert.Equal(expectedDue, proposed.Count == 1);
    }

    [Fact]
    public async Task ProposesAYearlyItemOnlyInItsStartMonth()
    {
        _probe.RecurringItemManager.Add(CreateItem("Hausrat", RecurringInterval.Yearly, startMonth: 3));

        List<ProposedBooking> proposedInApril = await _probe.Workflow.GetProposedBookingsAsync(
            new DateOnly(2026, 4, 1),
            TestContext.Current.CancellationToken);
        List<ProposedBooking> proposedInMay = await _probe.Workflow.GetProposedBookingsAsync(
            new DateOnly(2026, 5, 1),
            TestContext.Current.CancellationToken);

        Assert.Single(proposedInApril);
        Assert.Empty(proposedInMay);
    }

    [Fact]
    public async Task DoesNotProposeAnInactiveItem()
    {
        RecurringItem item = CreateItem("Fitnessstudio", RecurringInterval.Monthly);
        item.Active = false;
        _probe.RecurringItemManager.Add(item);

        List<ProposedBooking> proposed = await _probe.Workflow.GetProposedBookingsAsync(
            new DateOnly(2026, 9, 1),
            TestContext.Current.CancellationToken);

        Assert.Empty(proposed);
    }

    [Fact]
    public async Task DoesNotProposeAnOccurrenceThatWasAlreadyBooked()
    {
        RecurringItem miete = CreateItem("Miete", RecurringInterval.Monthly, dueDay: 5);
        _probe.RecurringItemManager.Add(miete);
        _probe.BookingManager.Add(new Booking
        {
            Date = new DateOnly(2026, 9, 5), Type = miete.Type, CategoryId = miete.CategoryId, Amount = miete.Amount,
            Status = BookingStatus.Booked, RecurringItemId = miete.Id
        });

        List<ProposedBooking> proposed = await _probe.Workflow.GetProposedBookingsAsync(
            new DateOnly(2026, 9, 1),
            TestContext.Current.CancellationToken);

        Assert.Empty(proposed);
    }

    private static RecurringItem CreateItem(
        string name,
        RecurringInterval interval,
        int dueDay = 1,
        int startMonth = 0)
        => new()
        {
            Name = name, Type = CategoryType.Expense, CategoryId = Guid.CreateVersion7(), Amount = 58m,
            Interval = interval, DueDay = dueDay, StartMonth = startMonth
        };
}
