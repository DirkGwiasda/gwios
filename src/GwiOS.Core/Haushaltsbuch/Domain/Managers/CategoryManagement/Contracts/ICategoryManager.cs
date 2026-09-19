using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;

/// <summary>
/// Manages the categories of the Haushaltsbuch: creates, changes, reads and deletes them, and rejects categories
/// that are invalid.
/// </summary>
public interface ICategoryManager
{
    /// <summary>
    /// Validates and stores the given new category and returns it. Throws a <see cref="CategoryValidationException"/>
    /// if the category is invalid.
    /// </summary>
    Task<Category> CreateCategoryAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and stores the given changed category. Throws a <see cref="CategoryValidationException"/> if the
    /// category is invalid, or a <see cref="CategoryNotFoundException"/> if no category with its ID is stored.
    /// </summary>
    Task UpdateCategoryAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all stored categories, ordered by type and then by name. The list is empty if no categories are
    /// stored.
    /// </summary>
    Task<List<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the category with the given ID. Throws a <see cref="CategoryHasChildrenException"/> if another
    /// category still has it as its parent. Does nothing if no category with the given ID is stored.
    /// </summary>
    Task DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
}
