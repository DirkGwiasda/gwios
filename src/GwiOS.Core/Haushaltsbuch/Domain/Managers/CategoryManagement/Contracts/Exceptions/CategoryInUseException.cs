namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when a category cannot be deleted because it is still used by at least one booking or recurring item.
/// </summary>
/// <param name="categoryId">The ID of the category that is still in use.</param>
public sealed class CategoryInUseException(Guid categoryId)
    : Exception($"Category '{categoryId}' cannot be deleted because it is still used by a booking or recurring item.")
{
    /// <summary>
    /// The ID of the category that is still in use.
    /// </summary>
    public Guid CategoryId { get; } = categoryId;
}
