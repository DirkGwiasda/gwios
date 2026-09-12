using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// Single-line text input in the GwiOS style. It reports every keystroke through <see cref="ValueChanged"/>, trims
/// the text when the user leaves the field, flags a required but empty field once the user has left it, submits with
/// Enter and clears the text with Escape.
/// </summary>
public partial class GwiOSTextBox
{
    private bool _isTouched;

    /// <summary>
    /// The current text. Never <c>null</c>; empty if nothing is entered.
    /// </summary>
    [Parameter]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Raised with the new text on every change, so the value can be bound with <c>@bind-Value</c>.
    /// </summary>
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    /// <summary>
    /// Raised when the user presses Enter, typically to submit the form the field belongs to.
    /// </summary>
    [Parameter]
    public EventCallback OnEnter { get; set; }

    /// <summary>
    /// Hint shown while the field is empty; also used as accessible name if <see cref="AriaLabel"/> is not set.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Accessible name of the field, or <c>null</c> to use the <see cref="Placeholder"/>.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Maximum number of characters the user can enter, or <c>null</c> for no limit.
    /// </summary>
    [Parameter]
    public int? MaxLength { get; set; }

    /// <summary>
    /// Whether the field must not be empty. An empty required field is flagged once the user has left it.
    /// </summary>
    [Parameter]
    public bool IsRequired { get; set; }

    /// <summary>
    /// Message shown as tooltip while a required field is empty.
    /// </summary>
    [Parameter]
    public string RequiredMessage { get; set; } = "Bitte einen Wert eingeben.";

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

    private bool IsInvalid
        => IsRequired && _isTouched && string.IsNullOrWhiteSpace(Value);

    private async Task OnInputAsync(ChangeEventArgs eventArgs)
        => await UpdateValueAsync(eventArgs.Value?.ToString() ?? string.Empty);

    // The change event fires when the user leaves the field after changing it.
    private async Task OnChangeAsync(ChangeEventArgs eventArgs)
    {
        _isTouched = true;
        await UpdateValueAsync((eventArgs.Value?.ToString() ?? string.Empty).Trim());
    }

    private async Task OnKeyDownAsync(KeyboardEventArgs eventArgs)
    {
        if (eventArgs.Key == "Enter")
        {
            await OnEnter.InvokeAsync();
        }
        else if (eventArgs.Key == "Escape")
        {
            await UpdateValueAsync(string.Empty);
        }
    }

    private async Task UpdateValueAsync(string value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
    }
}
