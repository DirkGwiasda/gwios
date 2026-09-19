namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when a category is rejected because it violates at least one validation rule.
/// </summary>
/// <param name="categoryId">The ID of the rejected category.</param>
/// <param name="validationErrors">One message per violated rule; never empty.</param>
public sealed class CategoryValidationException(Guid categoryId, IReadOnlyList<string> validationErrors)
    : Exception($"Category '{categoryId}' is invalid: {string.Join(" ", validationErrors)}")
{
    /// <summary>
    /// The ID of the rejected category.
    /// </summary>
    public Guid CategoryId { get; } = categoryId;

    /// <summary>
    /// One message per violated rule; never empty.
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; } = validationErrors;
}
