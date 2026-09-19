using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// Switches the month a Haushaltsbuch page shows data for, with a "Heute" shortcut back to the current month.
/// </summary>
public partial class GwiOSMonthSwitcher
{
    private static readonly string[] MonthNames =
    [
        "Januar", "Februar", "März", "April", "Mai", "Juni",
        "Juli", "August", "September", "Oktober", "November", "Dezember"
    ];

    /// <summary>
    /// The first day of the shown month.
    /// </summary>
    [Parameter, EditorRequired]
    public DateOnly Month { get; set; }

    /// <summary>
    /// Raised with the first day of the newly selected month, so it can be bound with <c>@bind-Month</c>.
    /// </summary>
    [Parameter]
    public EventCallback<DateOnly> MonthChanged { get; set; }

    [Inject]
    private TimeProvider TimeProvider { get; set; } = default!;

    private string Label
        => $"{MonthNames[Month.Month - 1]} {Month.Year}";

    private async Task GoToPreviousMonthAsync()
        => await ChangeMonthAsync(Month.AddMonths(-1));

    private async Task GoToNextMonthAsync()
        => await ChangeMonthAsync(Month.AddMonths(1));

    private async Task GoToCurrentMonthAsync()
    {
        DateOnly today = DateOnly.FromDateTime(TimeProvider.GetLocalNow().DateTime);
        await ChangeMonthAsync(new DateOnly(today.Year, today.Month, 1));
    }

    private async Task ChangeMonthAsync(DateOnly month)
        => await MonthChanged.InvokeAsync(new DateOnly(month.Year, month.Month, 1));
}
