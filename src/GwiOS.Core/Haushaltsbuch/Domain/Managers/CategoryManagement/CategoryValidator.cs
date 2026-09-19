using System.Text.RegularExpressions;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement;

internal sealed partial class CategoryValidator(ILogger<CategoryValidator> logger) : ICategoryValidator
{
    // Root categories count as level 1, so at most 4 further levels of sub-categories are allowed below them.
    private const int MaxDepth = 5;

    private readonly ILogger<CategoryValidator> _logger = logger;

    public bool IsValid(
        Category category,
        IReadOnlyList<Category> existingCategories,
        out List<string> validationErrors)
    {
        validationErrors = CollectValidationErrors(category, existingCategories);
        if (validationErrors.Count == 0)
        {
            return true;
        }

        LogValidationErrors(category, validationErrors);
        return false;
    }

    private static List<string> CollectValidationErrors(Category category, IReadOnlyList<Category> existingCategories)
    {
        List<string> validationErrors = [];

        if (string.IsNullOrWhiteSpace(category.Name))
        {
            validationErrors.Add("The name must not be empty.");
        }

        if (!Enum.IsDefined(category.Type))
        {
            validationErrors.Add("The type is not a known category type.");
        }

        ValidateColor(category, validationErrors);
        ValidateParent(category, existingCategories, validationErrors);
        ValidateNameIsUniqueAmongSiblings(category, existingCategories, validationErrors);

        return validationErrors;
    }

    private static void ValidateColor(Category category, List<string> validationErrors)
    {
        if (category.ParentId is null)
        {
            if (!IsValidColor(category.Color))
            {
                validationErrors.Add("The color of a root category must be a valid hex code (e.g. '#3399ff').");
            }

            return;
        }

        if (category.Color is not null)
        {
            validationErrors.Add("A sub-category must not have its own color.");
        }
    }

    private static void ValidateParent(
        Category category,
        IReadOnlyList<Category> existingCategories,
        List<string> validationErrors)
    {
        if (category.ParentId is not Guid parentId)
        {
            return;
        }

        if (parentId == category.Id)
        {
            validationErrors.Add("A category cannot be its own parent category.");
            return;
        }

        Dictionary<Guid, Category> categoriesById = existingCategories.ToDictionary(existing => existing.Id);
        if (!categoriesById.TryGetValue(parentId, out Category? parent))
        {
            validationErrors.Add("The parent category is not stored.");
            return;
        }

        if (parent.Type != category.Type)
        {
            validationErrors.Add("The type must match the parent category's type.");
        }

        ValidateDepth(category, parent, categoriesById, validationErrors);
    }

    // Walks from the chosen parent up to its root, counting the levels above the category being saved. The walk also
    // detects a cycle: if it reaches the category itself, the chosen parent is one of its own descendants.
    private static void ValidateDepth(
        Category category,
        Category parent,
        Dictionary<Guid, Category> categoriesById,
        List<string> validationErrors)
    {
        int depth = 2; // the category being saved, plus the parent
        HashSet<Guid> visited = [category.Id, parent.Id];
        Guid? currentId = parent.ParentId;
        while (currentId is Guid id)
        {
            if (id == category.Id)
            {
                validationErrors.Add("The parent category must not be a sub-category of this category.");
                return;
            }

            if (!visited.Add(id) || !categoriesById.TryGetValue(id, out Category? ancestor))
            {
                break;
            }

            depth++;
            currentId = ancestor.ParentId;
        }

        if (depth > MaxDepth)
        {
            validationErrors.Add($"Categories can be nested at most {MaxDepth} levels deep.");
        }
    }

    private static void ValidateNameIsUniqueAmongSiblings(
        Category category,
        IReadOnlyList<Category> existingCategories,
        List<string> validationErrors)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return;
        }

        bool isDuplicate = existingCategories.Any(existing =>
            (existing.Id != category.Id)
            && (existing.ParentId == category.ParentId)
            && string.Equals(existing.Name.Trim(), category.Name.Trim(), StringComparison.OrdinalIgnoreCase));

        if (isDuplicate)
        {
            validationErrors.Add("Another category at the same level already uses this name.");
        }
    }

    private static bool IsValidColor(string? color)
        => (color is not null) && HexColorRegex().IsMatch(color);

    private void LogValidationErrors(Category category, List<string> validationErrors)
        => _logger.LogWarning(
            "Category is invalid.",
            new Dictionary<string, string>
            {
                ["CategoryId"] = category.Id.ToString(),
                ["ValidationErrors"] = string.Join(" ", validationErrors)
            });

    [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
    private static partial Regex HexColorRegex();
}
