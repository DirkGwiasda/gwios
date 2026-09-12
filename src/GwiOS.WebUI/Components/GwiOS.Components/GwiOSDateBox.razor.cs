using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// Date input in the GwiOS style, bound to a <see cref="DateOnly"/> that is <c>null</c> while no date is chosen. The
/// browser's date picker handles the entry; Enter submits and Escape clears the date.
/// </summary>
public partial class GwiOSDateBox
{
    // The format HTML date inputs exchange their value in, independent of the displayed format.
    private const string HtmlDateFormat = "yyyy-MM-dd";

    /// <summary>
    /// The chosen date, or <c>null</c> if none is chosen.
    /// </summary>
    [Parameter]
    public DateOnly? Value { get; set; }

    /// <summary>
    /// Raised with the new date whenever the user changes it, so the value can be bound with <c>@bind-Value</c>.
    /// </summary>
    [Parameter]
    public EventCallback<DateOnly?> ValueChanged { get; set; }

    /// <summary>
    /// Raised when the user presses Enter, typically to submit the form the field belongs to.
    /// </summary>
    [Parameter]
    public EventCallback OnEnter { get; set; }

    /// <summary>
    /// Accessible name of the field.
    /// </summary>
    [Parameter]
    public string AriaLabel { get; set; } = "Datum";

    /// <summary>
    /// Additional CSS classes for the input element, or <c>null</c> for none.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Any further HTML attributes, which are passed on to the input element unchanged.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private string FormattedValue
        => Value?.ToString(HtmlDateFormat, CultureInfo.InvariantCulture) ?? string.Empty;

    private async Task OnChangeAsync(ChangeEventArgs eventArgs)
        => await UpdateValueAsync(ParseDate(eventArgs.Value?.ToString()));

    private async Task OnKeyDownAsync(KeyboardEventArgs eventArgs)
    {
        if (eventArgs.Key == "Enter")
        {
            await OnEnter.InvokeAsync();
        }
        else if (eventArgs.Key == "Escape")
        {
            await UpdateValueAsync(null);
        }
    }

    private async Task UpdateValueAsync(DateOnly? value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
    }

    // The browser sends an empty string while the date is incomplete or cleared.
    private static DateOnly? ParseDate(string? htmlDate)
        => DateOnly.TryParseExact(
            htmlDate,
            HtmlDateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateOnly date)
            ? date
            : null;
}
