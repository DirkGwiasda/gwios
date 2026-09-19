using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;

/// <summary>
/// Stands in for <see cref="IRecurringItemRepository"/> by keeping recurring items in memory. It orders, updates
/// and deletes like the real repository.
/// </summary>
public sealed class RecurringItemRepositoryFake : IRecurringItemRepository
{
    private readonly List<RecurringItem> _recurringItems = [];

    public Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task InsertAsync(RecurringItem recurringItem, CancellationToken cancellationToken = default)
    {
        _recurringItems.Add(recurringItem);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(RecurringItem recurringItem, CancellationToken cancellationToken = default)
    {
        int index = _recurringItems.FindIndex(stored => stored.Id == recurringItem.Id);
        if (index < 0)
        {
            throw new RecurringItemNotFoundException(recurringItem.Id);
        }

        _recurringItems[index] = recurringItem;
        return Task.CompletedTask;
    }

    public Task<List<RecurringItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_recurringItems.OrderBy(item => item.Type).ThenBy(item => item.Name).ToList());

    public Task DeleteAsync(Guid recurringItemId, CancellationToken cancellationToken = default)
    {
        _recurringItems.RemoveAll(item => item.Id == recurringItemId);
        return Task.CompletedTask;
    }
}
