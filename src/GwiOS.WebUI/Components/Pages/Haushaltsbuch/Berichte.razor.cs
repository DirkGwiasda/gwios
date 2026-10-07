using System.Globalization;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.HaushaltsbuchOverview.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.HaushaltsbuchOverview.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts.Models;
using GwiOS.WebUI.Api.Haushaltsbuch.Contracts;
using GwiOS.WebUI.Components.GwiOS.Components;
using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.Pages.Haushaltsbuch;

/// <summary>
/// Berichte tab of the Haushaltsbuch: Soll/Ist against the recurring items, an expense breakdown donut and an
/// income/expense trend, all over a selectable range of months, and a forecast of the recurring items from the current
/// month on, also offered as PDF.
/// </summary>
public partial class Berichte
{
    private static readonly string[] RangeItems = ["Aktuelles Jahr", "Letzte 6 Monate", "Letzte 12 Monate"];
    private static readonly string[] MonthNames =
    [
        "Januar", "Februar", "März", "April", "Mai", "Juni",
        "Juli", "August", "September", "Oktober", "November", "Dezember"
    ];
    private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");

    private string _range = "last12";
    private Guid? _donutRootId;
    private List<Category> _categories = [];
    private List<RecurringItem> _recurringItems = [];
    private List<Booking> _rangeBookings = [];
    private IReadOnlyList<RecurringItemForecastMonth> _forecastMonths = [];

    [Inject]
    private IHaushaltsbuchOverviewWorkflow OverviewWorkflow { get; set; } = default!;

    [Inject]
    private IRecurringItemForecastWorkflow ForecastWorkflow { get; set; } = default!;

    [Inject]
    private TimeProvider TimeProvider { get; set; } = default!;

    private string ForecastHint
        => _forecastMonths.Count == 0
            ? "Aktive Fixposten"
            : $"Aktive Fixposten, {MonthNames[_forecastMonths[0].Month.Month - 1]} {_forecastMonths[0].Month.Year} – "
                + $"{MonthNames[_forecastMonths[^1].Month.Month - 1]} {_forecastMonths[^1].Month.Year}";

    private string SelectedRangeItem
        => _range switch { "year" => "Aktuelles Jahr", "last6" => "Letzte 6 Monate", _ => "Letzte 12 Monate" };

    private List<DateOnly> RangeMonthStarts
    {
        get
        {
            DateOnly today = DateOnly.FromDateTime(TimeProvider.GetLocalNow().DateTime);
            int rangeMonths = _range switch { "last6" => 6, "year" => today.Month, _ => 12 };
            DateOnly currentMonthStart = new(today.Year, today.Month, 1);
            return [.. Enumerable.Range(0, rangeMonths).Select(offset => currentMonthStart.AddMonths(-(rangeMonths - 1 - offset)))];
        }
    }

    private string RangeInfo
    {
        get
        {
            List<DateOnly> months = RangeMonthStarts;
            return $"{months.Count} Monate bis {MonthNames[months[^1].Month - 1]} {months[^1].Year}";
        }
    }

    private List<SollIstRow> SollIstRows
        => ComputeSollIstRows();

    private List<GwiOSDonutSegment> DonutSegments
        => ComputeDonutSegments();

    private string DonutHint
        => _donutRootId is Guid rootId
            ? $"{_categories.SingleOrDefault(category => category.Id == rootId)?.Name} · zum Zurückgehen Eintrag anklicken"
            : "Hauptkategorien · zum Aufklappen anklicken";

    private List<GwiOSLineSeries> LineSeries
        => ComputeLineSeries();

    private List<string> LineLabels
        => [.. RangeMonthStarts.Select(month => MonthNames[month.Month - 1][..3])];

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
        await LoadForecastAsync();
    }

    private async Task SelectRangeAsync(string item)
    {
        _range = item switch { "Aktuelles Jahr" => "year", "Letzte 6 Monate" => "last6", _ => "last12" };
        _donutRootId = null;
        await LoadAsync();
    }

    private void DrillIntoCategory(Guid categoryId)
        => _donutRootId = _donutRootId is null ? categoryId : null;

    private static string FormatAmount(decimal value)
        => value.ToString("C", GermanCulture);

    private List<SollIstRow> ComputeSollIstRows()
    {
        int rangeMonths = RangeMonthStarts.Count;
        List<Category> expenseRoots =
            [.. _categories.Where(category => (category.ParentId is null) && (category.Type == CategoryType.Expense))];
        List<(string Label, decimal Plan, decimal Actual)> raw = [];
        foreach (Category root in expenseRoots)
        {
            HashSet<Guid> descendantIds = CollectWithDescendants(root.Id);
            decimal plan = _recurringItems
                .Where(item => item.Active && (item.Type == CategoryType.Expense) && descendantIds.Contains(item.CategoryId))
                .Sum(GetMonthlyEquivalent) * rangeMonths;
            decimal actual = _rangeBookings
                .Where(booking => (booking.Type == CategoryType.Expense) && descendantIds.Contains(booking.CategoryId))
                .Sum(booking => booking.Amount);
            if ((plan > 0) || (actual > 0))
            {
                raw.Add((root.Name, plan, actual));
            }
        }

        decimal max = raw.Count == 0 ? 1 : Math.Max(1, raw.Max(row => Math.Max(row.Plan, row.Actual)));
        return [.. raw.Select(row => new SollIstRow(
            row.Label,
            row.Plan,
            row.Actual,
            (int)Math.Round((row.Plan / max) * 100),
            (int)Math.Round((row.Actual / max) * 100)))];
    }

    private List<GwiOSDonutSegment> ComputeDonutSegments()
    {
        List<Category> expenseRoots =
            [.. _categories.Where(category => (category.ParentId is null) && (category.Type == CategoryType.Expense))];
        List<Category> parents = _donutRootId is Guid rootId
            ? [.. _categories.Where(category => category.ParentId == rootId)]
            : expenseRoots;

        List<GwiOSDonutSegment> segments = [];
        foreach (Category parent in parents)
        {
            HashSet<Guid> descendantIds = CollectWithDescendants(parent.Id);
            decimal value = _rangeBookings
                .Where(booking => (booking.Type == CategoryType.Expense) && descendantIds.Contains(booking.CategoryId))
                .Sum(booking => booking.Amount);
            if (value > 0)
            {
                segments.Add(new GwiOSDonutSegment(parent.Id, parent.Name, value, ResolveColor(parent)));
            }
        }

        return [.. segments.OrderByDescending(segment => segment.Value)];
    }

    private List<GwiOSLineSeries> ComputeLineSeries()
    {
        List<decimal> income = [];
        List<decimal> expense = [];
        foreach (DateOnly monthStart in RangeMonthStarts)
        {
            DateOnly monthEnd = new(monthStart.Year, monthStart.Month, DateTime.DaysInMonth(monthStart.Year, monthStart.Month));
            income.Add(_rangeBookings
                .Where(booking => (booking.Type == CategoryType.Income) && (booking.Date >= monthStart) && (booking.Date <= monthEnd))
                .Sum(booking => booking.Amount));
            expense.Add(_rangeBookings
                .Where(booking => (booking.Type == CategoryType.Expense) && (booking.Date >= monthStart) && (booking.Date <= monthEnd))
                .Sum(booking => booking.Amount));
        }

        return
        [
            new GwiOSLineSeries("Einnahmen", "#00ffa3", income),
            new GwiOSLineSeries("Ausgaben", "#ff4d6d", expense)
        ];
    }

    private static decimal GetMonthlyEquivalent(RecurringItem item)
        => item.Interval switch
        {
            RecurringInterval.Weekly => item.Amount * 52 / 12,
            RecurringInterval.Monthly => item.Amount,
            RecurringInterval.Quarterly => item.Amount / 3,
            RecurringInterval.Yearly => item.Amount / 12,
            _ => item.Amount
        };

    // Only root categories carry a color; a sub-category shows the color of its root.
    private string ResolveColor(Category category)
    {
        Dictionary<Guid, Category> categoriesById = _categories.ToDictionary(candidate => candidate.Id);
        Category current = category;
        int guard = 0;
        while ((current.ParentId is Guid parentId)
            && categoriesById.TryGetValue(parentId, out Category? parent)
            && (guard++ < 8))
        {
            current = parent;
        }

        return current.Color ?? "#7c93b3";
    }

    private HashSet<Guid> CollectWithDescendants(Guid categoryId)
    {
        HashSet<Guid> result = [categoryId];
        List<Guid> pending = [categoryId];
        int index = 0;
        while (index < pending.Count)
        {
            Guid currentId = pending[index++];
            foreach (Category child in _categories.Where(category => category.ParentId == currentId))
            {
                if (result.Add(child.Id))
                {
                    pending.Add(child.Id);
                }
            }
        }

        return result;
    }

    private async Task LoadAsync()
    {
        List<DateOnly> months = RangeMonthStarts;
        DateOnly fromDate = months[0];
        DateOnly lastMonthStart = months[^1];
        DateOnly toDate = new(
            lastMonthStart.Year,
            lastMonthStart.Month,
            DateTime.DaysInMonth(lastMonthStart.Year, lastMonthStart.Month));
        HaushaltsbuchOverviewData overview = await OverviewWorkflow.GetOverviewAsync(fromDate, toDate);
        _categories = overview.Categories;
        _recurringItems = overview.RecurringItems;
        _rangeBookings = overview.Bookings;
    }

    // The forecast always starts with the current month; the range selection of the other reports does not apply.
    private async Task LoadForecastAsync()
    {
        DateTimeOffset now = TimeProvider.GetLocalNow();
        RecurringItemForecastData forecast = await ForecastWorkflow.GetForecastAsync(
            new DateOnly(now.Year, now.Month, 1),
            IRecurringItemForecastPdfEndpoint.MonthCount);
        _forecastMonths = forecast.Months;
    }

    private sealed record SollIstRow(string Label, decimal Plan, decimal Actual, int PlanPercentage, int ActualPercentage);
}
