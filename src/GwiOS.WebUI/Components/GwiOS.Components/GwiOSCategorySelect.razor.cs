using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// Dropdown listing the categories of one type as a tree, indented by depth. Used to pick the category of a
/// booking or recurring item, and the parent category of another category.
/// </summary>
public partial class GwiOSCategorySelect
{
    /// <summary>
    /// All stored categories; only those of <see cref="Type"/> are offered.
    /// </summary>
    [Parameter, EditorRequired]
    public IReadOnlyList<Category> Categories { get; set; } = [];

    /// <summary>
    /// The type of category to offer.
    /// </summary>
    [Parameter, EditorRequired]
    public CategoryType Type { get; set; }

    /// <summary>
    /// The selected category, or <c>null</c> if none is selected (only possible while <see cref="EmptyOptionLabel"/>
    /// is set).
    /// </summary>
    [Parameter]
    public Guid? SelectedCategoryId { get; set; }

    /// <summary>
    /// Raised with the newly selected category, so the selection can be bound with
    /// <c>@bind-SelectedCategoryId</c>.
    /// </summary>
    [Parameter]
    public EventCallback<Guid?> SelectedCategoryIdChanged { get; set; }

    /// <summary>
    /// A category whose descendants (and itself) are left out of the list, to prevent a category from becoming
    /// its own ancestor; or <c>null</c> to offer every category of <see cref="Type"/>.
    /// </summary>
    [Parameter]
    public Guid? ExcludeCategoryId { get; set; }

    /// <summary>
    /// Label of a leading option with an empty value, meaning "no category"; or <c>null</c> to require a
    /// selection.
    /// </summary>
    [Parameter]
    public string? EmptyOptionLabel { get; set; }

    /// <summary>
    /// Accessible name of the dropdown.
    /// </summary>
    [Parameter]
    public string AriaLabel { get; set; } = "Kategorie";

    /// <summary>
    /// Any further HTML attributes, such as <c>id</c>, which are passed on to the <c>select</c> element unchanged.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private string SelectedValue
        => SelectedCategoryId?.ToString() ?? string.Empty;

    private List<CategoryOption> Options
        => BuildOptions();

    private List<CategoryOption> BuildOptions()
    {
        HashSet<Guid> excludedIds = ExcludeCategoryId is Guid excludeId ? CollectWithDescendants(excludeId) : [];
        List<CategoryOption> options = [];
        WalkCategoryTree(null, 0, excludedIds, options);
        return options;
    }

    private void WalkCategoryTree(Guid? parentId, int depth, HashSet<Guid> excludedIds, List<CategoryOption> options)
    {
        foreach (Category category in Categories
            .Where(category => (category.Type == Type) && (category.ParentId == parentId) && !excludedIds.Contains(category.Id))
            .OrderBy(category => category.Name))
        {
            string prefix = depth == 0 ? string.Empty : new string(' ', depth * 4) + "› ";
            options.Add(new CategoryOption(category, prefix + category.Name));
            WalkCategoryTree(category.Id, depth + 1, excludedIds, options);
        }
    }

    private HashSet<Guid> CollectWithDescendants(Guid categoryId)
    {
        HashSet<Guid> result = [categoryId];
        List<Guid> pending = [categoryId];
        int index = 0;
        while (index < pending.Count)
        {
            Guid currentId = pending[index++];
            foreach (Category child in Categories.Where(category => category.ParentId == currentId))
            {
                if (result.Add(child.Id))
                {
                    pending.Add(child.Id);
                }
            }
        }

        return result;
    }

    private async Task OnChangeAsync(ChangeEventArgs eventArgs)
    {
        string? value = eventArgs.Value?.ToString();
        SelectedCategoryId = string.IsNullOrEmpty(value) ? null : Guid.Parse(value);
        await SelectedCategoryIdChanged.InvokeAsync(SelectedCategoryId);
    }

    private sealed record CategoryOption(Category Category, string Label);
}
