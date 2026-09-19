namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when an operation requires a stored category, but no category with the given ID is stored.
/// </summary>
/// <param name="categoryId">The ID that no stored category has.</param>
public sealed class CategoryNotFoundException(Guid categoryId)
    : Exception($"No category with ID '{categoryId}' is stored.")
{
    /// <summary>
    /// The ID that no stored category has.
    /// </summary>
    public Guid CategoryId { get; } = categoryId;
}
