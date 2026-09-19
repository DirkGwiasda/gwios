using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;

/// <summary>
/// Stands in for <see cref="IRecurringItemManager"/> for the workflow tests. It keeps recurring items in memory and
/// does not validate.
/// </summary>
public sealed class RecurringItemManagerFake : IRecurringItemManager
{
    private readonly List<RecurringItem> _recurringItems = [];

    /// <summary>
    /// Stores a recurring item directly, as if it had been created before the test.
    /// </summary>
    public void Add(RecurringItem recurringItem)
        => _recurringItems.Add(recurringItem);

    public Task<RecurringItem> CreateRecurringItemAsync(
        RecurringItem recurringItem,
        CancellationToken cancellationToken = default)
    {
        _recurringItems.Add(recurringItem);
        return Task.FromResult(recurringItem);
    }

    public Task UpdateRecurringItemAsync(RecurringItem recurringItem, CancellationToken cancellationToken = default)
    {
        int index = _recurringItems.FindIndex(stored => stored.Id == recurringItem.Id);
        _recurringItems[index] = recurringItem;
        return Task.CompletedTask;
    }

    public Task<List<RecurringItem>> GetAllRecurringItemsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_recurringItems.OrderBy(item => item.Type).ThenBy(item => item.Name).ToList());

    public Task DeleteRecurringItemAsync(Guid recurringItemId, CancellationToken cancellationToken = default)
    {
        _recurringItems.RemoveAll(item => item.Id == recurringItemId);
        return Task.CompletedTask;
    }
}
