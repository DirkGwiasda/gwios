using LogViewerUnderTest = GwiOS.WebUI.Components.Pages.Admin.LogViewer;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.LogViewer;

/// <summary>
/// Covers switching to another page of entries on <c>LogViewer</c>.
/// </summary>
public sealed class ChangePageAsyncTests : LogViewerProbe
{
    [Fact]
    public void ShowsTheEntriesOfTheChosenPage()
    {
        AddEntries("GwiOS", "Eintrag", 12);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        ClickPagerButton(page, "Weiter ›");

        Assert.Equal(["Eintrag 2", "Eintrag 1"], GetShownMessages(page));
        Assert.Equal("11–12 von 12", page.Find(".gwios-pager-range").TextContent);
    }
}
