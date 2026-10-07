using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts.Models;

namespace GwiOS.WebUI.Tests.TestInfrastructure;

/// <summary>
/// Creates forecasts of recurring items for the tests: an October 2026 with one income and two expenses.
/// </summary>
public sealed class TestRecurringItemForecasts
{
    public Guid GehaltCategoryId { get; } = Guid.NewGuid();

    public Guid MieteCategoryId { get; } = Guid.NewGuid();

    /// <summary>
    /// Returns October 2026: "Gehalt Anna" 1850 € on the 1st, "Miete" 1150 € on the 1st and "Strom" 98.50 € on the 5th.
    /// </summary>
    public RecurringItemForecastMonth CreateOctober()
        => new()
        {
            Month = new DateOnly(2026, 10, 1),
            Incomes = [CreateEntry("Gehalt Anna", 1, GehaltCategoryId, ["Gehalt"], 1850m)],
            Expenses =
            [
                CreateEntry("Miete", 1, MieteCategoryId, ["Wohnen", "Miete"], 1150m),
                CreateEntry("Strom", 5, MieteCategoryId, ["Wohnen", "Miete"], 98.5m)
            ],
            IncomeTotal = 1850m,
            ExpenseTotal = 1248.5m,
            Balance = 601.5m
        };

    /// <summary>
    /// Returns an empty month: nothing falls due.
    /// </summary>
    public static RecurringItemForecastMonth CreateEmpty(DateOnly month)
        => new() { Month = month, Incomes = [], Expenses = [], IncomeTotal = 0m, ExpenseTotal = 0m, Balance = 0m };

    private static RecurringItemForecastEntry CreateEntry(
        string name,
        int day,
        Guid categoryId,
        IReadOnlyList<string> categoryPath,
        decimal amount)
        => new()
        {
            RecurringItemId = Guid.NewGuid(),
            Date = new DateOnly(2026, 10, day),
            Name = name,
            CategoryId = categoryId,
            CategoryPath = categoryPath,
            Amount = amount
        };
}
