using GwiOSPagerUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSPager;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSPager;

/// <summary>
/// Covers choosing the page size on <c>GwiOSPager</c>.
/// </summary>
public sealed class SelectPageSizeAsyncTests : GwiOSPagerProbe
{
    [Fact]
    public void ReportsTheChosenPageSize()
    {
        IRenderedComponent<GwiOSPagerUnderTest> pager = RenderPager(totalCount: 134, pageNumber: 1);

        ClickButton(pager, "50");

        Assert.Equal([50], ReportedPageSizes);
        Assert.Empty(ReportedPageNumbers);
    }

    [Fact]
    public void ReportsNothing_WhenTheCurrentPageSizeIsChosen()
    {
        IRenderedComponent<GwiOSPagerUnderTest> pager = RenderPager(totalCount: 134, pageNumber: 1);

        ClickButton(pager, "10");

        Assert.Empty(ReportedPageSizes);
    }
}
