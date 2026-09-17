using GwiOSPagerUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSPager;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSPager;

/// <summary>
/// Covers stepping back one page on <c>GwiOSPager</c>.
/// </summary>
public sealed class GoToPreviousPageAsyncTests : GwiOSPagerProbe
{
    [Fact]
    public void ReportsThePreviousPageNumber()
    {
        IRenderedComponent<GwiOSPagerUnderTest> pager = RenderPager(totalCount: 134, pageNumber: 5);

        ClickButton(pager, "‹ Zurück");

        Assert.Equal([4], ReportedPageNumbers);
    }

    [Fact]
    public void ReportsTheSecondToLastPage_WhenTheCurrentPageNumberIsBeyondTheLastPage()
    {
        IRenderedComponent<GwiOSPagerUnderTest> pager = RenderPager(totalCount: 25, pageNumber: 9);

        ClickButton(pager, "‹ Zurück");

        Assert.Equal([2], ReportedPageNumbers);
    }
}
