using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// Page navigation below a paged list in the GwiOS style. It shows which entries the current page holds, lets the user
/// step to the previous or next page, jump to a page by its number and choose how many entries a page holds. With many
/// pages it shows the first, the last and the pages around the current one, with a gap in between.
/// </summary>
public partial class GwiOSPager
{
    // Up to this many pages every page number is shown; beyond it gaps replace the pages far from the current one.
    private const int MaxPageNumbersWithoutGaps = 7;

    /// <summary>
    /// The number of entries across all pages; zero or more.
    /// </summary>
    [Parameter, EditorRequired]
    public int TotalCount { get; set; }

    /// <summary>
    /// The number of the current page, counting from 1. A number beyond the last page is shown as the last page.
    /// </summary>
    [Parameter]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Raised with the number of the page the user wants to see, so the page number can be bound with
    /// <c>@bind-PageNumber</c>. Not raised for the current page.
    /// </summary>
    [Parameter]
    public EventCallback<int> PageNumberChanged { get; set; }

    /// <summary>
    /// The number of entries a page holds; at least 1.
    /// </summary>
    [Parameter]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Raised with the page size the user chooses, so the page size can be bound with <c>@bind-PageSize</c>. Not
    /// raised for the current page size.
    /// </summary>
    [Parameter]
    public EventCallback<int> PageSizeChanged { get; set; }

    /// <summary>
    /// The page sizes the user can choose from, in display order; each at least 1.
    /// </summary>
    [Parameter]
    public IReadOnlyList<int> PageSizes { get; set; } = [10, 25, 50];

    private int PageCount
        => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

    private int CurrentPageNumber
        => Math.Clamp(PageNumber, 1, PageCount);

    private bool IsOnFirstPage
        => (TotalCount == 0) || (CurrentPageNumber == 1);

    private bool IsOnLastPage
        => (TotalCount == 0) || (CurrentPageNumber == PageCount);

    private string RangeText
        => TotalCount == 0
            ? "Keine Einträge"
            : $"{FirstEntryNumber}–{LastEntryNumber} von {TotalCount}";

    private string CompactText
        => TotalCount == 0
            ? "Keine Einträge"
            : $"Seite {CurrentPageNumber} von {PageCount}";

    private int FirstEntryNumber
        => ((CurrentPageNumber - 1) * PageSize) + 1;

    private int LastEntryNumber
        => Math.Min(CurrentPageNumber * PageSize, TotalCount);

    // The page numbers to show in display order; null stands for a gap of pages that are left out.
    private List<int?> PageNumbers
        => PageCount <= MaxPageNumbersWithoutGaps
            ? [.. Enumerable.Range(1, PageCount).Select(number => (int?)number)]
            : CollectPageNumbersWithGaps();

    private List<int?> CollectPageNumbersWithGaps()
    {
        (int firstInner, int lastInner) = GetInnerPageRange();
        List<int?> pageNumbers = [1];
        if (firstInner > 2)
        {
            pageNumbers.Add(null);
        }

        pageNumbers.AddRange(Enumerable.Range(firstInner, lastInner - firstInner + 1).Select(number => (int?)number));
        if (lastInner < PageCount - 1)
        {
            pageNumbers.Add(null);
        }

        pageNumbers.Add(PageCount);
        return pageNumbers;
    }

    // The three pages shown between the first and the last page: around the current page, or next to the first or
    // last page while the current page is close to them.
    private (int FirstInner, int LastInner) GetInnerPageRange()
    {
        if (CurrentPageNumber <= 3)
        {
            return (2, 4);
        }

        if (CurrentPageNumber >= PageCount - 2)
        {
            return (PageCount - 3, PageCount - 1);
        }

        return (CurrentPageNumber - 1, CurrentPageNumber + 1);
    }

    private async Task GoToPageAsync(int pageNumber)
    {
        if (pageNumber == CurrentPageNumber)
        {
            return;
        }

        await PageNumberChanged.InvokeAsync(pageNumber);
    }

    private async Task GoToPreviousPageAsync()
        => await GoToPageAsync(Math.Max(1, CurrentPageNumber - 1));

    private async Task GoToNextPageAsync()
        => await GoToPageAsync(Math.Min(PageCount, CurrentPageNumber + 1));

    private async Task SelectPageSizeAsync(int pageSize)
    {
        if (pageSize == PageSize)
        {
            return;
        }

        await PageSizeChanged.InvokeAsync(pageSize);
    }
}
