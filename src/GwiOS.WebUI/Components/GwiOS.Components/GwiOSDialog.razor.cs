using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// Modal dialog in the GwiOS style: a panel with a title, content and optional action buttons above a dimmed page.
/// It takes the keyboard focus when it opens and asks to be closed through the close button, a click on the dimmed
/// page or Escape. On narrow screens it slides up from the bottom. The dialog is open as long as it is rendered.
/// </summary>
public partial class GwiOSDialog
{
    private readonly string _titleId = $"gwios-dialog-title-{Guid.NewGuid():N}";
    private ElementReference _dialogElement;

    /// <summary>
    /// The title shown in the header; also the accessible name of the dialog.
    /// </summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Additional content of the header, shown between the title and the close button, such as a badge; or
    /// <c>null</c> for none.
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderContent { get; set; }

    /// <summary>
    /// The content of the dialog. It scrolls if it is taller than the dialog.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The buttons shown in the footer, right-aligned; or <c>null</c> for a dialog without footer.
    /// </summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// Whether the user can close the dialog. While <c>false</c>, the close button is disabled and a click on the
    /// dimmed page or Escape does nothing, for example while the dialog saves.
    /// </summary>
    [Parameter]
    public bool CanClose { get; set; } = true;

    /// <summary>
    /// Raised when the user asks to close the dialog. The dialog stays open until the parent stops rendering it.
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _dialogElement.FocusAsync();
        }
    }

    private async Task CloseAsync()
    {
        if (!CanClose)
        {
            return;
        }

        await OnClose.InvokeAsync();
    }

    private async Task OnKeyDownAsync(KeyboardEventArgs eventArgs)
    {
        if (eventArgs.Key == "Escape")
        {
            await CloseAsync();
        }
    }
}
