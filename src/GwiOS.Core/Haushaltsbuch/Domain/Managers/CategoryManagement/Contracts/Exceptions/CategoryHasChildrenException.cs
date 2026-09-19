namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when a category cannot be deleted because at least one other category still has it as its parent.
/// </summary>
/// <param name="categoryId">The ID of the category that still has sub-categories.</param>
public sealed class CategoryHasChildrenException(Guid categoryId)
    : Exception($"Category '{categoryId}' cannot be deleted because it still has sub-categories.")
{
    /// <summary>
    /// The ID of the category that still has sub-categories.
    /// </summary>
    public Guid CategoryId { get; } = categoryId;
}
