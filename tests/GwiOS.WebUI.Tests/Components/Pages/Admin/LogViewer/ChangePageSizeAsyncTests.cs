using LogViewerUnderTest = GwiOS.WebUI.Components.Pages.Admin.LogViewer;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.LogViewer;

/// <summary>
/// Covers choosing how many entries a page of <c>LogViewer</c> holds.
/// </summary>
public sealed class ChangePageSizeAsyncTests : LogViewerProbe
{
    [Fact]
    public void ShowsAsManyEntriesAsTheChosenPageSize()
    {
        AddEntries("GwiOS", "Eintrag", 30);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        ClickPagerButton(page, "25");

        Assert.Equal(25, GetShownMessages(page).Count);
    }

    [Fact]
    public void ShowsTheFirstPage()
    {
        AddEntries("GwiOS", "Eintrag", 30);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();
        ClickPagerButton(page, "3");

        ClickPagerButton(page, "25");

        Assert.Equal("Eintrag 30", GetShownMessages(page)[0]);
    }
}
