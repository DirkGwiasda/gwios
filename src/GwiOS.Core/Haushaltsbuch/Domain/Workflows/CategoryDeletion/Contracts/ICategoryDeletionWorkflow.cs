using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions;

namespace GwiOS.Core.Haushaltsbuch.Domain.Workflows.CategoryDeletion.Contracts;

/// <summary>
/// Deletes a Haushaltsbuch category only if nothing still depends on it.
/// </summary>
public interface ICategoryDeletionWorkflow
{
    /// <summary>
    /// Deletes the category with the given ID. Throws a <see cref="CategoryInUseException"/> if a booking or
    /// recurring item still uses it, or a <see cref="CategoryHasChildrenException"/> if another category still has
    /// it as its parent. Does nothing if no category with the given ID is stored.
    /// </summary>
    Task DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
}
