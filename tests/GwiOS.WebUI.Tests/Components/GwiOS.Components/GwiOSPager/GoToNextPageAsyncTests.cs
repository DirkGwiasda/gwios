using GwiOSPagerUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSPager;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSPager;

/// <summary>
/// Covers stepping forward one page on <c>GwiOSPager</c>.
/// </summary>
public sealed class GoToNextPageAsyncTests : GwiOSPagerProbe
{
    [Fact]
    public void ReportsTheNextPageNumber()
    {
        IRenderedComponent<GwiOSPagerUnderTest> pager = RenderPager(totalCount: 134, pageNumber: 5);

        ClickButton(pager, "Weiter ›");

        Assert.Equal([6], ReportedPageNumbers);
    }
}
