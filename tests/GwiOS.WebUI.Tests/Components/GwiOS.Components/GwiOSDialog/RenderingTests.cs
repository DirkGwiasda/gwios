using AngleSharp.Dom;
using Microsoft.AspNetCore.Components;
using GwiOSDialogUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSDialog;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSDialog;

/// <summary>
/// Covers how <c>GwiOSDialog</c> renders its title, content and actions.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Fact]
    public void RendersAModalDialogNamedByItsTitle()
    {
        IRenderedComponent<GwiOSDialogUnderTest> dialog = Render<GwiOSDialogUnderTest>(parameters => parameters
            .Add(component => component.Title, "Log-Eintrag"));

        IElement element = dialog.Find("[role='dialog']");
        IElement title = dialog.Find(".gwios-dialog-title");
        Assert.Equal("true", element.GetAttribute("aria-modal"));
        Assert.Equal("Log-Eintrag", title.TextContent);
        Assert.Equal(title.Id, element.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void ShowsTheHeaderContentAndTheContent()
    {
        IRenderedComponent<GwiOSDialogUnderTest> dialog = Render<GwiOSDialogUnderTest>(parameters => parameters
            .Add(component => component.Title, "Log-Eintrag")
            .Add(component => component.HeaderContent, "<span class=\"badge\">Error</span>")
            .AddChildContent("<p class=\"content\">Details</p>"));

        Assert.Equal("Error", dialog.Find(".gwios-dialog-header .badge").TextContent);
        Assert.Equal("Details", dialog.Find(".gwios-dialog-body .content").TextContent);
    }

    [Fact]
    public void ShowsTheActionsInAFooter_WhenTheyAreSet()
    {
        IRenderedComponent<GwiOSDialogUnderTest> dialog = Render<GwiOSDialogUnderTest>(parameters => parameters
            .Add(component => component.Title, "Person bearbeiten")
            .Add(component => component.Actions, "<button>Speichern</button>"));

        Assert.Equal("Speichern", dialog.Find(".gwios-dialog-actions button").TextContent);
    }

    [Fact]
    public void ShowsNoFooter_WhenNoActionsAreSet()
    {
        IRenderedComponent<GwiOSDialogUnderTest> dialog = Render<GwiOSDialogUnderTest>(parameters => parameters
            .Add(component => component.Title, "Log-Eintrag"));

        Assert.Empty(dialog.FindAll(".gwios-dialog-actions"));
    }

    [Fact]
    public void DisablesTheCloseButton_WhileTheDialogCannotBeClosed()
    {
        IRenderedComponent<GwiOSDialogUnderTest> dialog = Render<GwiOSDialogUnderTest>(parameters => parameters
            .Add(component => component.Title, "Person bearbeiten")
            .Add(component => component.CanClose, false));

        Assert.True(dialog.Find(".gwios-dialog-close").HasAttribute("disabled"));
    }

    [Fact]
    public void TakesTheFocus_WhenItOpens()
    {
        IRenderedComponent<GwiOSDialogUnderTest> dialog = Render<GwiOSDialogUnderTest>(parameters => parameters
            .Add(component => component.Title, "Log-Eintrag"));

        ElementReference focusedElement =
            Assert.IsType<ElementReference>(JSInterop.VerifyFocusAsyncInvoke().Arguments[0]);
        Assert.Equal(
            focusedElement.Id,
            dialog.Find("[role='dialog']").GetAttribute("blazor:elementReference"));
    }
}
