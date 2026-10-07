using AngleSharp.Dom;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts.Models;
using GwiOS.WebUI.Tests.TestInfrastructure;
using GwiOSRecurringItemForecastMonthUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSRecurringItemForecastMonth;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSRecurringItemForecastMonth;

/// <summary>
/// Covers how <c>GwiOSRecurringItemForecastMonth</c> shows one month of the forecast.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    private readonly TestRecurringItemForecasts _forecasts = new();

    [Fact]
    public void ShowsTheMonthNameAndYearAsTitle()
    {
        IRenderedComponent<GwiOSRecurringItemForecastMonthUnderTest> month = RenderMonth(_forecasts.CreateOctober());

        Assert.Equal("Oktober 2026", month.Find("h3").TextContent);
    }

    [Fact]
    public void ShowsIncomeAndExpensesAsSeparateTablesWithTheirTotals()
    {
        IRenderedComponent<GwiOSRecurringItemForecastMonthUnderTest> month = RenderMonth(_forecasts.CreateOctober());

        IReadOnlyList<IElement> sections = month.FindAll(".gwios-forecast-section");
        Assert.Equal(["Einnahmen", "Ausgaben"], sections.Select(section => section.QuerySelector(".gwios-forecast-section-title")!.TextContent));
        Assert.Equal(["Gehalt Anna"], GetNames(sections[0]));
        Assert.Equal(["Miete", "Strom"], GetNames(sections[1]));
        Assert.Equal(FormatEuros(1248.5m), sections[1].QuerySelector(".gwios-forecast-section-header .gwios-amount")!.TextContent);
    }

    [Fact]
    public void ShowsDateNameCategoryAndAmountOfEachEntry()
    {
        IRenderedComponent<GwiOSRecurringItemForecastMonthUnderTest> month = RenderMonth(_forecasts.CreateOctober());

        IElement row = month.FindAll(".gwios-forecast-section")[1].QuerySelectorAll("tbody tr")[1];
        Assert.Equal("05.10.", row.QuerySelector(".gwios-forecast-date")!.TextContent);
        Assert.Equal("Strom", row.QuerySelector(".gwios-forecast-name")!.TextContent);
        Assert.Equal("Wohnen › Miete", row.QuerySelector(".gwios-category-label-path")!.TextContent);
        Assert.Equal(FormatEuros(98.5m), row.QuerySelector(".gwios-forecast-amount")!.TextContent.Trim());
    }

    [Theory]
    [InlineData(601.5, "+ ", "gwios-amount--success")]
    [InlineData(-20, "– ", "gwios-amount--danger")]
    public void ShowsTheBalanceWithSignAndColor(decimal balance, string expectedSign, string expectedClass)
    {
        RecurringItemForecastMonth october = _forecasts.CreateOctober();
        RecurringItemForecastMonth withBalance = new()
        {
            Month = october.Month,
            Incomes = october.Incomes,
            Expenses = october.Expenses,
            IncomeTotal = october.IncomeTotal,
            ExpenseTotal = october.ExpenseTotal,
            Balance = balance
        };

        IRenderedComponent<GwiOSRecurringItemForecastMonthUnderTest> month = RenderMonth(withBalance);

        IElement amount = month.Find(".gwios-forecast-month-balance .gwios-amount");
        Assert.Equal(expectedSign + FormatEuros(Math.Abs(balance)), amount.TextContent.Trim());
        Assert.True(amount.ClassList.Contains(expectedClass));
    }

    [Fact]
    public void SaysSoInsteadOfATable_WhenNothingFallsDue()
    {
        IRenderedComponent<GwiOSRecurringItemForecastMonthUnderTest> month =
            RenderMonth(TestRecurringItemForecasts.CreateEmpty(new DateOnly(2026, 10, 1)));

        Assert.Empty(month.FindAll("table"));
        Assert.Equal(
            ["Keine fälligen Einnahmen.", "Keine fälligen Ausgaben."],
            month.FindAll(".gwios-forecast-empty").Select(text => text.TextContent));
    }

    private IRenderedComponent<GwiOSRecurringItemForecastMonthUnderTest> RenderMonth(RecurringItemForecastMonth forecastMonth)
    {
        Category wohnen = new() { Name = "Wohnen", Type = CategoryType.Expense, Color = "#ff4d6d" };
        Category miete = new() { Id = _forecasts.MieteCategoryId, Name = "Miete", Type = CategoryType.Expense, ParentId = wohnen.Id };
        Category gehalt = new() { Id = _forecasts.GehaltCategoryId, Name = "Gehalt", Type = CategoryType.Income, Color = "#00ffa3" };
        return Render<GwiOSRecurringItemForecastMonthUnderTest>(parameters => parameters
            .Add(component => component.Month, forecastMonth)
            .Add(component => component.Categories, [wohnen, miete, gehalt]));
    }

    private static IEnumerable<string> GetNames(IElement section)
        => section.QuerySelectorAll(".gwios-forecast-name").Select(name => name.TextContent);

    private static string FormatEuros(decimal value)
        => value.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
}
