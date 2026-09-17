using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using LogViewerUnderTest = GwiOS.WebUI.Components.Pages.Admin.LogViewer;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.LogViewer;

/// <summary>
/// Covers refreshing the entries on <c>LogViewer</c>.
/// </summary>
public sealed class ReloadEntriesAsyncTests : LogViewerProbe
{
    [Fact]
    public void ShowsEntriesWrittenAfterThePageWasOpened()
    {
        AddEntry("GwiOS", "Gestartet", LogLevel.Information, Morning);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();
        AddEntry("GwiOS", "ToDo angelegt", LogLevel.Information, Morning.AddMinutes(5));

        ClickReloadButton(page);

        Assert.Equal(["ToDo angelegt", "Gestartet"], GetShownMessages(page));
    }

    [Fact]
    public void StaysOnTheCurrentPage()
    {
        AddEntries("GwiOS", "Eintrag", 12);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();
        ClickPagerButton(page, "2");

        ClickReloadButton(page);

        Assert.Equal(["Eintrag 2", "Eintrag 1"], GetShownMessages(page));
    }

    [Fact]
    public void ShowsTheLastPage_WhenTheCurrentPageNoLongerExists()
    {
        AddEntries("Backup", "Alt", 2);
        AddEntries("GwiOS", "Neu", 10);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();
        ClickPagerButton(page, "2");
        LogEntryManager.Remove("Backup");

        ClickReloadButton(page);

        Assert.Equal(10, GetShownMessages(page).Count);
        Assert.Equal("1–10 von 10", page.Find(".gwios-pager-range").TextContent);
    }

    private static void ClickReloadButton(IRenderedComponent<LogViewerUnderTest> page)
        => page.FindAll(".gwios-log-actions button").Single(button => button.TextContent.Trim() == "Aktualisieren").Click();
}
