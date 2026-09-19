using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;

/// <summary>
/// Checks the values of a single category. Unlike a validator that only looks at one entity in isolation, a category
/// is only valid in the context of the other stored categories: its parent must exist and have the same type, its
/// name must be unique among its siblings, and it must not nest the category tree too deep or introduce a cycle.
/// </summary>
public interface ICategoryValidator
{
    /// <summary>
    /// Returns whether the given category is valid, considering the given other stored categories (which, for an
    /// update, still contains the category's previous state). <paramref name="validationErrors"/> receives one
    /// message per violated rule; it is empty if the category is valid.
    /// </summary>
    bool IsValid(
        Category category,
        IReadOnlyList<Category> existingCategories,
        out List<string> validationErrors);
}
