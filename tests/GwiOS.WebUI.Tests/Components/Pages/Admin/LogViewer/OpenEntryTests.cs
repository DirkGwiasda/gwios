using AngleSharp.Dom;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using LogViewerUnderTest = GwiOS.WebUI.Components.Pages.Admin.LogViewer;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.LogViewer;

/// <summary>
/// Covers opening the details of an entry on <c>LogViewer</c>.
/// </summary>
public sealed class OpenEntryTests : LogViewerProbe
{
    [Fact]
    public void ShowsTheDetailsOfTheClickedEntryInADialog()
    {
        AddEntry("GwiOS", "Gestartet", LogLevel.Information, Morning);
        AddEntry("GwiOS", "Synchronisation fehlgeschlagen.\nHTTP 503.", LogLevel.Error, Morning.AddMinutes(1));
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        page.Find("tr.gwios-log-row").Click();

        IElement dialog = page.Find("[role='dialog']");
        Assert.Equal("Log-Eintrag", dialog.QuerySelector(".gwios-dialog-title")!.TextContent);
        Assert.Equal(
            "Synchronisation fehlgeschlagen.\nHTTP 503.",
            dialog.QuerySelector(".gwios-log-details-message")!.TextContent);
    }

    [Fact]
    public void ShowsTheLevelOfTheEntryInTheHeaderOfTheDialog()
    {
        AddEntry("GwiOS", "Fehlgeschlagen", LogLevel.Error, Morning);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        page.Find("tr.gwios-log-row").Click();

        Assert.Equal("Error", page.Find(".gwios-dialog-header .gwios-log-level--large").TextContent);
    }

    [Fact]
    public void ShowsTheDetails_WhenEnterIsPressedOnAnEntry()
    {
        AddEntry("GwiOS", "Gestartet", LogLevel.Information, Morning);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        page.Find("tr.gwios-log-row").KeyDown("Enter");

        Assert.Equal("Gestartet", page.Find(".gwios-log-details-message").TextContent);
    }

    [Fact]
    public void ShowsNoDetails_WhenAnotherKeyIsPressedOnAnEntry()
    {
        AddEntry("GwiOS", "Gestartet", LogLevel.Information, Morning);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        page.Find("tr.gwios-log-row").KeyDown("a");

        Assert.Empty(page.FindAll("[role='dialog']"));
    }
}
