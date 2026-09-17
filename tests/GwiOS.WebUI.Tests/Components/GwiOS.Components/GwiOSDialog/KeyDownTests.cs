using GwiOSDialogUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSDialog;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSDialog;

/// <summary>
/// Covers how <c>GwiOSDialog</c> reacts to keys pressed inside it.
/// </summary>
public sealed class KeyDownTests : BunitContext
{
    private int _closeCount;

    [Fact]
    public void RaisesOnClose_WhenEscapeIsPressed()
    {
        IRenderedComponent<GwiOSDialogUnderTest> dialog = RenderDialog(canClose: true);

        dialog.Find("[role='dialog']").KeyDown("Escape");

        Assert.Equal(1, _closeCount);
    }

    [Fact]
    public void RaisesNoOnClose_WhenAnotherKeyIsPressed()
    {
        IRenderedComponent<GwiOSDialogUnderTest> dialog = RenderDialog(canClose: true);

        dialog.Find("[role='dialog']").KeyDown("Enter");

        Assert.Equal(0, _closeCount);
    }

    [Fact]
    public void RaisesNoOnClose_WhenEscapeIsPressedWhileTheDialogCannotBeClosed()
    {
        IRenderedComponent<GwiOSDialogUnderTest> dialog = RenderDialog(canClose: false);

        dialog.Find("[role='dialog']").KeyDown("Escape");

        Assert.Equal(0, _closeCount);
    }

    private IRenderedComponent<GwiOSDialogUnderTest> RenderDialog(bool canClose)
        => Render<GwiOSDialogUnderTest>(parameters => parameters
            .Add(component => component.Title, "Person bearbeiten")
            .Add(component => component.CanClose, canClose)
            .Add(component => component.OnClose, () => _closeCount++));
}
