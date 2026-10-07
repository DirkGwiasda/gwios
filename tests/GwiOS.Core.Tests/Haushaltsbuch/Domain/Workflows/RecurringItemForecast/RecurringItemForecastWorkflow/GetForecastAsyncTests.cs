using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.RecurringItemForecastWorkflow;

/// <summary>
/// Covers <c>RecurringItemForecastWorkflow.GetForecastAsync</c>.
/// </summary>
public sealed class GetForecastAsyncTests
{
    private readonly RecurringItemForecastWorkflowProbe _probe = new();

    [Fact]
    public async Task ReturnsTheRequestedNumberOfConsecutiveMonthsStartingWithTheFirstDayOfTheGivenMonth()
    {
        RecurringItemForecastData forecast = await GetForecastAsync(new DateOnly(2026, 11, 17), monthCount: 3);

        Assert.Equal(
            [new DateOnly(2026, 11, 1), new DateOnly(2026, 12, 1), new DateOnly(2027, 1, 1)],
            forecast.Months.Select(month => month.Month));
    }

    [Fact]
    public async Task SplitsTheDueItemsIntoIncomeAndExpenses()
    {
        _probe.AddItem("Miete", _probe.Miete, 1150m);
        _probe.AddItem("Gehalt Anna", _probe.Gehalt, 1850m);

        RecurringItemForecastMonth month = Assert.Single((await GetForecastAsync(new DateOnly(2026, 10, 1), 1)).Months);

        Assert.Equal(["Gehalt Anna"], month.Incomes.Select(entry => entry.Name));
        Assert.Equal(["Miete"], month.Expenses.Select(entry => entry.Name));
    }

    [Fact]
    public async Task DescribesEachDueDateWithItemCategoryPathAndAmount()
    {
        RecurringItem miete = _probe.AddItem("Miete", _probe.Miete, 1150m, dueDay: 3);

        RecurringItemForecastMonth month = Assert.Single((await GetForecastAsync(new DateOnly(2026, 10, 1), 1)).Months);

        RecurringItemForecastEntry entry = Assert.Single(month.Expenses);
        Assert.Equal(miete.Id, entry.RecurringItemId);
        Assert.Equal(new DateOnly(2026, 10, 3), entry.Date);
        Assert.Equal("Miete", entry.Name);
        Assert.Equal(_probe.Miete.Id, entry.CategoryId);
        Assert.Equal(["Wohnen", "Miete"], entry.CategoryPath);
        Assert.Equal(1150m, entry.Amount);
    }

    [Fact]
    public async Task ListsAWeeklyItemOncePerDueDate()
    {
        _probe.AddItem("Wocheneinkauf", _probe.Wohnen, 100m, RecurringInterval.Weekly);

        RecurringItemForecastMonth month = Assert.Single((await GetForecastAsync(new DateOnly(2026, 10, 1), 1)).Months);

        Assert.Equal([1, 8, 15, 22], month.Expenses.Select(entry => entry.Date.Day));
        Assert.Equal(400m, month.ExpenseTotal);
    }

    [Fact]
    public async Task ListsAQuarterlyItemOnlyInTheMonthsItFallsDue()
    {
        // StartMonth 9 is October (zero-based): due in October and January, not in November and December.
        _probe.AddItem("Kfz-Versicherung", _probe.Wohnen, 312m, RecurringInterval.Quarterly, dueDay: 1, startMonth: 9);

        RecurringItemForecastData forecast = await GetForecastAsync(new DateOnly(2026, 10, 1), 4);

        Assert.Equal([1, 0, 0, 1], forecast.Months.Select(month => month.Expenses.Count));
    }

    [Fact]
    public async Task LeavesOutInactiveItems()
    {
        RecurringItem item = _probe.AddItem("Fitnessstudio", _probe.Wohnen, 39.9m);
        item.Active = false;

        RecurringItemForecastMonth month = Assert.Single((await GetForecastAsync(new DateOnly(2026, 10, 1), 1)).Months);

        Assert.Empty(month.Expenses);
    }

    [Fact]
    public async Task OrdersTheEntriesOfAMonthByDateAndThenByName()
    {
        _probe.AddItem("Strom", _probe.Wohnen, 98m, dueDay: 5);
        _probe.AddItem("Miete", _probe.Miete, 1150m, dueDay: 1);
        _probe.AddItem("Internet", _probe.Wohnen, 44.9m, dueDay: 5);

        RecurringItemForecastMonth month = Assert.Single((await GetForecastAsync(new DateOnly(2026, 10, 1), 1)).Months);

        Assert.Equal(["Miete", "Internet", "Strom"], month.Expenses.Select(entry => entry.Name));
    }

    [Fact]
    public async Task SumsIncomeAndExpensesAndTheirBalancePerMonth()
    {
        _probe.AddItem("Miete", _probe.Miete, 1150m);
        _probe.AddItem("Strom", _probe.Wohnen, 98.5m);
        _probe.AddItem("Gehalt Anna", _probe.Gehalt, 1850m);

        RecurringItemForecastMonth month = Assert.Single((await GetForecastAsync(new DateOnly(2026, 10, 1), 1)).Months);

        Assert.Equal(1850m, month.IncomeTotal);
        Assert.Equal(1248.5m, month.ExpenseTotal);
        Assert.Equal(601.5m, month.Balance);
    }

    [Fact]
    public async Task ReturnsAnEmptyCategoryPath_WhenTheCategoryIsUnknown()
    {
        _probe.RecurringItemManager.Add(new RecurringItem
        {
            Name = "Ohne Kategorie",
            Type = CategoryType.Expense,
            CategoryId = Guid.NewGuid(),
            Amount = 5m,
            Interval = RecurringInterval.Monthly
        });

        RecurringItemForecastMonth month = Assert.Single((await GetForecastAsync(new DateOnly(2026, 10, 1), 1)).Months);

        Assert.Empty(Assert.Single(month.Expenses).CategoryPath);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ThrowsArgumentOutOfRangeException_WhenTheMonthCountIsLessThanOne(int monthCount)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => GetForecastAsync(new DateOnly(2026, 10, 1), monthCount));
    }

    [Fact]
    public async Task LogsTheForecastAsDebug()
    {
        await GetForecastAsync(new DateOnly(2026, 10, 1), 6);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Debug, logEntry.LogLevel);
        Assert.Equal("2026-10-01", logEntry.ContextData["FirstMonth"]);
        Assert.Equal("6", logEntry.ContextData["MonthCount"]);
    }

    private Task<RecurringItemForecastData> GetForecastAsync(DateOnly firstMonth, int monthCount)
        => _probe.Workflow.GetForecastAsync(firstMonth, monthCount, TestContext.Current.CancellationToken);
}
