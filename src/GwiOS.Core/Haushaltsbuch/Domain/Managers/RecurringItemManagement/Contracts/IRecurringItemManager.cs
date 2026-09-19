using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;

/// <summary>
/// Manages the recurring items of the Haushaltsbuch: creates, changes, reads and deletes them, and rejects
/// recurring items that are invalid.
/// </summary>
public interface IRecurringItemManager
{
    /// <summary>
    /// Validates and stores the given new recurring item and returns it. Throws a
    /// <see cref="RecurringItemValidationException"/> if the recurring item is invalid.
    /// </summary>
    Task<RecurringItem> CreateRecurringItemAsync(RecurringItem recurringItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and stores the given changed recurring item. Throws a <see cref="RecurringItemValidationException"/>
    /// if the recurring item is invalid, or a <see cref="RecurringItemNotFoundException"/> if no recurring item with
    /// its ID is stored.
    /// </summary>
    Task UpdateRecurringItemAsync(RecurringItem recurringItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all stored recurring items, ordered by type and then by name. The list is empty if no recurring
    /// items are stored.
    /// </summary>
    Task<List<RecurringItem>> GetAllRecurringItemsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the recurring item with the given ID. Does nothing if no such recurring item is stored.
    /// </summary>
    Task DeleteRecurringItemAsync(Guid recurringItemId, CancellationToken cancellationToken = default);
}
