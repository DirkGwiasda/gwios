using GwiOSPagerUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSPager;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSPager;

/// <summary>
/// Base of the <c>GwiOSPager</c> tests: renders the pager and records the page numbers and page sizes it reports.
/// </summary>
public abstract class GwiOSPagerProbe : BunitContext
{
    /// <summary>
    /// The page numbers the pager reported through <c>PageNumberChanged</c>, in order.
    /// </summary>
    protected List<int> ReportedPageNumbers { get; } = [];

    /// <summary>
    /// The page sizes the pager reported through <c>PageSizeChanged</c>, in order.
    /// </summary>
    protected List<int> ReportedPageSizes { get; } = [];

    /// <summary>
    /// Renders the pager for the given number of entries, showing the given page with ten entries per page.
    /// </summary>
    protected IRenderedComponent<GwiOSPagerUnderTest> RenderPager(int totalCount, int pageNumber)
        => Render<GwiOSPagerUnderTest>(parameters => parameters
            .Add(component => component.TotalCount, totalCount)
            .Add(component => component.PageNumber, pageNumber)
            .Add(component => component.PageSize, 10)
            .Add(component => component.PageNumberChanged, (int number) => ReportedPageNumbers.Add(number))
            .Add(component => component.PageSizeChanged, (int size) => ReportedPageSizes.Add(size)));

    /// <summary>
    /// Clicks the button of the pager with the given text.
    /// </summary>
    protected static void ClickButton(IRenderedComponent<GwiOSPagerUnderTest> pager, string text)
        => pager.FindAll("button").First(button => button.TextContent.Trim() == text).Click();
}
