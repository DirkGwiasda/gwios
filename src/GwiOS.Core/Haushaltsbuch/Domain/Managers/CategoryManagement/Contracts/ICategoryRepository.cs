using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;

/// <summary>
/// Persists, reads and deletes Haushaltsbuch categories in the underlying storage.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Creates the tables required for categories unless they already exist. The database itself must already
    /// exist. The operation is idempotent: calling it against existing tables changes nothing.
    /// </summary>
    Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores the given new category.
    /// </summary>
    Task InsertAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Overwrites the stored category with the ID of the given category. Throws a
    /// <see cref="CategoryNotFoundException"/> if no such category is stored.
    /// </summary>
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all stored categories, ordered by type and then by name. The category tree itself is not reflected in
    /// the order; callers that need it build it from <see cref="Category.ParentId"/>. The list is empty if no
    /// categories are stored.
    /// </summary>
    Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the category with the given ID. Does nothing if no such category is stored.
    /// </summary>
    Task DeleteAsync(Guid categoryId, CancellationToken cancellationToken = default);
}
