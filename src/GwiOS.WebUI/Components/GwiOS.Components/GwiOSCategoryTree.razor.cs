using System.Globalization;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// Recursive tree of the categories of one type, with per-category sums, collapsing and the actions of the
/// Kategorien tab. Renders itself for each level of the tree.
/// </summary>
public partial class GwiOSCategoryTree
{
    private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>
    /// The categories of one type, flat; the tree is built from their <see cref="Category.ParentId"/>.
    /// </summary>
    [Parameter, EditorRequired]
    public IReadOnlyList<Category> Categories { get; set; } = [];

    /// <summary>
    /// The sum to show for each category ID, e.g. of the bookings in the selected month. A missing entry is shown
    /// as no sum.
    /// </summary>
    [Parameter, EditorRequired]
    public IReadOnlyDictionary<Guid, decimal> Sums { get; set; } = new Dictionary<Guid, decimal>();

    /// <summary>
    /// The IDs of the categories whose children are currently collapsed.
    /// </summary>
    [Parameter, EditorRequired]
    public IReadOnlySet<Guid> CollapsedCategoryIds { get; set; } = new HashSet<Guid>();

    /// <summary>
    /// Raised with the category whose children should collapse or expand.
    /// </summary>
    [Parameter, EditorRequired]
    public EventCallback<Guid> OnToggleCollapse { get; set; }

    /// <summary>
    /// Raised with the parent category a new sub-category should be added under.
    /// </summary>
    [Parameter, EditorRequired]
    public EventCallback<Guid?> OnAddChild { get; set; }

    /// <summary>
    /// Raised with the category to edit.
    /// </summary>
    [Parameter, EditorRequired]
    public EventCallback<Category> OnEdit { get; set; }

    /// <summary>
    /// Raised with the category to delete, after the user confirmed.
    /// </summary>
    [Parameter, EditorRequired]
    public EventCallback<Category> OnDelete { get; set; }

    /// <summary>
    /// The parent whose direct children this level of the tree renders; <c>null</c> renders the root categories.
    /// Set only by the component itself when it renders a deeper level.
    /// </summary>
    [Parameter]
    public Guid? ParentId { get; set; }

    /// <summary>
    /// How many levels deep this level of the tree is; 0 for the root categories. Set only by the component itself.
    /// </summary>
    [Parameter]
    public int Depth { get; set; }

    private static string GetIndentStyle(int depth)
        => $"padding-left: {14 + (depth * 20)}px;";

    private bool HasChildren(Category category)
        => Categories.Any(candidate => candidate.ParentId == category.Id);

    private string FormatSum(Category category)
    {
        decimal sum = Sums.GetValueOrDefault(category.Id);
        return sum == 0 ? "—" : sum.ToString("C", GermanCulture);
    }

    // Only root categories carry a color; a sub-category shows the color of its root.
    private string ResolveColor(Category category)
    {
        Dictionary<Guid, Category> categoriesById = Categories.ToDictionary(candidate => candidate.Id);
        Category current = category;
        int guard = 0;
        while ((current.ParentId is Guid parentId)
            && categoriesById.TryGetValue(parentId, out Category? parent)
            && (guard++ < 8))
        {
            current = parent;
        }

        return current.Color ?? "#7c93b3";
    }
}
