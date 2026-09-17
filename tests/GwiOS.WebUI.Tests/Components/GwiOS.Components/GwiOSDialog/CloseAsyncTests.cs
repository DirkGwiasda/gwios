using GwiOSDialogUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSDialog;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSDialog;

/// <summary>
/// Covers how <c>GwiOSDialog</c> asks to be closed through the close button and a click on the dimmed page.
/// </summary>
public sealed class CloseAsyncTests : BunitContext
{
    private int _closeCount;

    [Fact]
    public void RaisesOnClose_WhenTheCloseButtonIsClicked()
    {
        IRenderedComponent<GwiOSDialogUnderTest> dialog = RenderDialog(canClose: true);

        dialog.Find(".gwios-dialog-close").Click();

        Assert.Equal(1, _closeCount);
    }

    [Fact]
    public void RaisesOnClose_WhenTheDimmedPageIsClicked()
    {
        IRenderedComponent<GwiOSDialogUnderTest> dialog = RenderDialog(canClose: true);

        dialog.Find(".gwios-dialog-backdrop").Click();

        Assert.Equal(1, _closeCount);
    }

    [Fact]
    public void RaisesNoOnClose_WhenTheDimmedPageIsClickedWhileTheDialogCannotBeClosed()
    {
        IRenderedComponent<GwiOSDialogUnderTest> dialog = RenderDialog(canClose: false);

        dialog.Find(".gwios-dialog-backdrop").Click();

        Assert.Equal(0, _closeCount);
    }

    private IRenderedComponent<GwiOSDialogUnderTest> RenderDialog(bool canClose)
        => Render<GwiOSDialogUnderTest>(parameters => parameters
            .Add(component => component.Title, "Person bearbeiten")
            .Add(component => component.CanClose, canClose)
            .Add(component => component.OnClose, () => _closeCount++));
}
