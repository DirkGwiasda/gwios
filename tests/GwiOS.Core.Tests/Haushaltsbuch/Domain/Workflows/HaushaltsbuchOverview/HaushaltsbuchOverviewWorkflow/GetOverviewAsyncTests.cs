using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.HaushaltsbuchOverview.Contracts.Models;
using GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using HaushaltsbuchOverviewWorkflowUnderTest =
    GwiOS.Core.Haushaltsbuch.Domain.Workflows.HaushaltsbuchOverview.HaushaltsbuchOverviewWorkflow;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Workflows.HaushaltsbuchOverview.HaushaltsbuchOverviewWorkflow;

/// <summary>
/// Covers <c>HaushaltsbuchOverviewWorkflow.GetOverviewAsync</c>.
/// </summary>
public sealed class GetOverviewAsyncTests
{
    private readonly CategoryManagerFake _categoryManager = new();
    private readonly RecurringItemManagerFake _recurringItemManager = new();
    private readonly BookingManagerFake _bookingManager = new();
    private readonly HaushaltsbuchOverviewWorkflowUnderTest _workflow;

    public GetOverviewAsyncTests()
    {
        _workflow = new HaushaltsbuchOverviewWorkflowUnderTest(_categoryManager, _recurringItemManager, _bookingManager);
    }

    [Fact]
    public async Task ReturnsAllCategoriesAndRecurringItems()
    {
        Category category = new() { Name = "Wohnen", Type = CategoryType.Expense, Color = "#ff4d6d" };
        RecurringItem recurringItem = new()
        {
            Name = "Miete", Type = CategoryType.Expense, CategoryId = category.Id, Amount = 1150m,
            Interval = RecurringInterval.Monthly
        };
        _categoryManager.Add(category);
        _recurringItemManager.Add(recurringItem);

        HaushaltsbuchOverviewData overview = await _workflow.GetOverviewAsync(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 30),
            TestContext.Current.CancellationToken);

        Assert.Same(category, Assert.Single(overview.Categories));
        Assert.Same(recurringItem, Assert.Single(overview.RecurringItems));
    }

    [Fact]
    public async Task ReturnsOnlyBookingsWithinTheGivenRange()
    {
        Booking inRange = CreateBooking(new DateOnly(2026, 9, 15));
        Booking outOfRange = CreateBooking(new DateOnly(2026, 10, 1));
        _bookingManager.Add(inRange);
        _bookingManager.Add(outOfRange);

        HaushaltsbuchOverviewData overview = await _workflow.GetOverviewAsync(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 30),
            TestContext.Current.CancellationToken);

        Assert.Same(inRange, Assert.Single(overview.Bookings));
    }

    private static Booking CreateBooking(DateOnly date)
        => new()
        {
            Date = date, Type = CategoryType.Expense, CategoryId = Guid.CreateVersion7(), Amount = 58m,
            Status = BookingStatus.Booked
        };
}
