using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// Shows a category with the color of its root category and its full path, e.g. "Haushalt › Lebensmittel".
/// </summary>
public partial class GwiOSCategoryLabel
{
    private const string FallbackColor = "#7c93b3";
    private const string MissingPath = "—";

    // Categories nest at most 5 levels deep; the limit only protects against a cycle in inconsistent data.
    private const int MaxDepth = 8;

    private string _path = MissingPath;
    private string _color = FallbackColor;

    /// <summary>
    /// All categories, flat; the path is built from their <see cref="Category.ParentId"/>.
    /// </summary>
    [Parameter, EditorRequired]
    public IReadOnlyList<Category> Categories { get; set; } = [];

    /// <summary>
    /// The ID of the category to show.
    /// </summary>
    [Parameter, EditorRequired]
    public Guid CategoryId { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        List<Category> lineage = CollectLineage();
        _path = lineage.Count == 0 ? MissingPath : string.Join(" › ", lineage.Select(category => category.Name));
        _color = lineage.FirstOrDefault()?.Color ?? FallbackColor;
    }

    // The category and its ancestors, from the root category down to the category itself.
    private List<Category> CollectLineage()
    {
        Dictionary<Guid, Category> categoriesById = Categories.ToDictionary(category => category.Id);
        List<Category> lineage = [];
        Guid? currentId = CategoryId;
        while ((currentId is Guid id) && categoriesById.TryGetValue(id, out Category? category) && (lineage.Count < MaxDepth))
        {
            lineage.Insert(0, category);
            currentId = category.ParentId;
        }

        return lineage;
    }
}
