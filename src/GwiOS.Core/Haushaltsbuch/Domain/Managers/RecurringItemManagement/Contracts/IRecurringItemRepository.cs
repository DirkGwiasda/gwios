using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;

/// <summary>
/// Persists, reads and deletes recurring items in the underlying storage.
/// </summary>
public interface IRecurringItemRepository
{
    /// <summary>
    /// Creates the tables required for recurring items unless they already exist. The database itself must already
    /// exist. The operation is idempotent: calling it against existing tables changes nothing.
    /// </summary>
    Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores the given new recurring item.
    /// </summary>
    Task InsertAsync(RecurringItem recurringItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Overwrites the stored recurring item with the ID of the given recurring item. Throws a
    /// <see cref="RecurringItemNotFoundException"/> if no such recurring item is stored.
    /// </summary>
    Task UpdateAsync(RecurringItem recurringItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all stored recurring items, ordered by type and then by name. The list is empty if no recurring
    /// items are stored.
    /// </summary>
    Task<List<RecurringItem>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the recurring item with the given ID. Does nothing if no such recurring item is stored.
    /// </summary>
    Task DeleteAsync(Guid recurringItemId, CancellationToken cancellationToken = default);
}
