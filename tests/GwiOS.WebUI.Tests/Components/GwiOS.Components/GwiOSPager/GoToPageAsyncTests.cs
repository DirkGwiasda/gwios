using GwiOSPagerUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSPager;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSPager;

/// <summary>
/// Covers jumping to a page by its number on <c>GwiOSPager</c>.
/// </summary>
public sealed class GoToPageAsyncTests : GwiOSPagerProbe
{
    [Fact]
    public void ReportsTheClickedPageNumber()
    {
        IRenderedComponent<GwiOSPagerUnderTest> pager = RenderPager(totalCount: 134, pageNumber: 1);

        ClickButton(pager, "14");

        Assert.Equal([14], ReportedPageNumbers);
    }

    [Fact]
    public void ReportsNothing_WhenTheCurrentPageIsClicked()
    {
        IRenderedComponent<GwiOSPagerUnderTest> pager = RenderPager(totalCount: 134, pageNumber: 2);

        ClickButton(pager, "2");

        Assert.Empty(ReportedPageNumbers);
    }
}
