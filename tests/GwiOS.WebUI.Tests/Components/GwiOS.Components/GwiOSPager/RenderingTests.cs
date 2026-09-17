using AngleSharp.Dom;
using GwiOSPagerUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSPager;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSPager;

/// <summary>
/// Covers what <c>GwiOSPager</c> shows for a number of entries and a current page.
/// </summary>
public sealed class RenderingTests : GwiOSPagerProbe
{
    [Fact]
    public void ShowsWhichEntriesTheCurrentPageHolds()
    {
        IRenderedComponent<GwiOSPagerUnderTest> pager = RenderPager(totalCount: 134, pageNumber: 2);

        Assert.Equal("11–20 von 134", pager.Find(".gwios-pager-range").TextContent);
        Assert.Equal("Seite 2 von 14", pager.Find(".gwios-pager-compact").TextContent);
    }

    [Fact]
    public void ShowsTheRemainingEntriesOnTheLastPage()
    {
        IRenderedComponent<GwiOSPagerUnderTest> pager = RenderPager(totalCount: 134, pageNumber: 14);

        Assert.Equal("131–134 von 134", pager.Find(".gwios-pager-range").TextContent);
    }

    [Fact]
    public void SaysThereAreNoEntriesAndDisablesBothSteps_WhenTheTotalIsZero()
    {
        IRenderedComponent<GwiOSPagerUnderTest> pager = RenderPager(totalCount: 0, pageNumber: 1);

        Assert.Equal("Keine Einträge", pager.Find(".gwios-pager-range").TextContent);
        Assert.Equal("Keine Einträge", pager.Find(".gwios-pager-compact").TextContent);
        Assert.All(pager.FindAll(".gwios-pager-step"), step => Assert.True(step.HasAttribute("disabled")));
    }

    [Fact]
    public void ShowsAPageNumberBeyondTheLastPageAsTheLastPage()
    {
        IRenderedComponent<GwiOSPagerUnderTest> pager = RenderPager(totalCount: 25, pageNumber: 9);

        Assert.Equal("21–25 von 25", pager.Find(".gwios-pager-range").TextContent);
        Assert.Equal("3", pager.Find(".gwios-pager-number--current").TextContent.Trim());
    }

    [Theory]
    [InlineData(70, 1, "1 2 3 4 5 6 7")]
    [InlineData(134, 1, "1 2 3 4 … 14")]
    [InlineData(134, 3, "1 2 3 4 … 14")]
    [InlineData(134, 4, "1 … 3 4 5 … 14")]
    [InlineData(134, 7, "1 … 6 7 8 … 14")]
    [InlineData(134, 11, "1 … 10 11 12 … 14")]
    [InlineData(134, 12, "1 … 11 12 13 14")]
    [InlineData(134, 14, "1 … 11 12 13 14")]
    public void ShowsTheFirstTheLastAndThePagesAroundTheCurrentOneWithGapsInBetween(
        int totalCount,
        int pageNumber,
        string expectedPageNumbers)
    {
        IRenderedComponent<GwiOSPagerUnderTest> pager = RenderPager(totalCount, pageNumber);

        IEnumerable<string> shownPageNumbers = pager.Find(".gwios-pager-numbers").Children
            .Select(item => item.TextContent.Trim());
        Assert.Equal(expectedPageNumbers, string.Join(" ", shownPageNumbers));
    }

    [Fact]
    public void MarksTheCurrentPage()
    {
        IRenderedComponent<GwiOSPagerUnderTest> pager = RenderPager(totalCount: 134, pageNumber: 2);

        IElement currentPage = pager.Find(".gwios-pager-number--current");
        Assert.Equal("2", currentPage.TextContent.Trim());
        Assert.Equal("page", currentPage.GetAttribute("aria-current"));
        Assert.Single(pager.FindAll("[aria-current]"));
    }

    [Fact]
    public void DisablesOnlyTheStepBack_OnTheFirstPage()
    {
        IRenderedComponent<GwiOSPagerUnderTest> pager = RenderPager(totalCount: 134, pageNumber: 1);

        IReadOnlyList<IElement> steps = pager.FindAll(".gwios-pager-step");
        Assert.True(steps[0].HasAttribute("disabled"));
        Assert.False(steps[1].HasAttribute("disabled"));
    }

    [Fact]
    public void DisablesOnlyTheStepForward_OnTheLastPage()
    {
        IRenderedComponent<GwiOSPagerUnderTest> pager = RenderPager(totalCount: 134, pageNumber: 14);

        IReadOnlyList<IElement> steps = pager.FindAll(".gwios-pager-step");
        Assert.False(steps[0].HasAttribute("disabled"));
        Assert.True(steps[1].HasAttribute("disabled"));
    }

    [Fact]
    public void OffersTheDefaultPageSizesAndMarksTheCurrentOne()
    {
        IRenderedComponent<GwiOSPagerUnderTest> pager = RenderPager(totalCount: 134, pageNumber: 1);

        IReadOnlyList<IElement> sizes = pager.FindAll(".gwios-pager-size");
        Assert.Equal(["10", "25", "50"], sizes.Select(size => size.TextContent.Trim()));
        Assert.Equal(["true", "false", "false"], sizes.Select(size => size.GetAttribute("aria-pressed")));
    }

    [Fact]
    public void OffersTheGivenPageSizes()
    {
        IRenderedComponent<GwiOSPagerUnderTest> pager = Render<GwiOSPagerUnderTest>(parameters => parameters
            .Add(component => component.TotalCount, 10)
            .Add(component => component.PageSize, 5)
            .Add(component => component.PageSizes, [5, 20]));

        Assert.Equal(["5", "20"], pager.FindAll(".gwios-pager-size").Select(size => size.TextContent.Trim()));
    }
}
