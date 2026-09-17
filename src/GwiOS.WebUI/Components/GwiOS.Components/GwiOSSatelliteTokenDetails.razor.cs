using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// Shows a new satellite token once, with the warning that it cannot be read again and a button that copies it to the
/// clipboard of the browser.
/// </summary>
public partial class GwiOSSatelliteTokenDetails
{
    private bool _isCopied;
    private bool _hasCopyFailed;

    /// <summary>
    /// The name of the satellite the token belongs to.
    /// </summary>
    [Parameter, EditorRequired]
    public string SatelliteName { get; set; } = string.Empty;

    /// <summary>
    /// The plain token.
    /// </summary>
    [Parameter, EditorRequired]
    public string Token { get; set; } = string.Empty;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    // The clipboard API is only available in secure contexts (HTTPS or localhost) and may be denied by the browser.
    private async Task CopyTokenAsync()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", Token);
            _isCopied = true;
            _hasCopyFailed = false;
        }
        catch (JSException)
        {
            _isCopied = false;
            _hasCopyFailed = true;
        }
    }
}
