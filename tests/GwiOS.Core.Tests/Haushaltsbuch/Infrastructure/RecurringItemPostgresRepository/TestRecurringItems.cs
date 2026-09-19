using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.CategoryPostgresRepository;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.RecurringItemPostgresRepository;

/// <summary>
/// Creates recurring items, and the categories a foreign key requires them to have, for the tests against the
/// PostgreSQL test database, and deletes everything it created when it is disposed (recurring items before their
/// categories, as the foreign key requires). The database may contain recurring items of other tests, so tests
/// only look at the recurring items they created themselves.
/// </summary>
public sealed class TestRecurringItems : IAsyncDisposable
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IRecurringItemRepository _recurringItemRepository;
    private readonly TestCategories _testCategories;
    private readonly List<Guid> _recurringItemIds = [];

    public TestRecurringItems(ICategoryRepository categoryRepository, IRecurringItemRepository recurringItemRepository)
    {
        _categoryRepository = categoryRepository;
        _recurringItemRepository = recurringItemRepository;
        _testCategories = new TestCategories(categoryRepository);
    }

    /// <summary>
    /// Creates and stores a new expense category, then returns a new, not yet stored, monthly expense recurring
    /// item for it, with a unique name.
    /// </summary>
    public async Task<RecurringItem> CreateAsync(CancellationToken cancellationToken = default)
    {
        Category category = _testCategories.Create();
        await _categoryRepository.InsertAsync(category, cancellationToken);
        RecurringItem recurringItem = new()
        {
            Name = CreateUniqueName(), Type = category.Type, CategoryId = category.Id, Amount = 1150m,
            Interval = RecurringInterval.Monthly
        };
        _recurringItemIds.Add(recurringItem.Id);
        return recurringItem;
    }

    /// <summary>
    /// Returns a name that no other recurring item uses, prefixed so that leftovers are recognizable as test data.
    /// </summary>
    public string CreateUniqueName()
        => $"test-{Guid.NewGuid():N}";

    /// <summary>
    /// Returns the stored recurring item with the given ID, or <c>null</c> if no such recurring item is stored.
    /// </summary>
    public async Task<RecurringItem?> FindStoredAsync(Guid recurringItemId)
        => (await _recurringItemRepository.GetAllAsync()).SingleOrDefault(item => item.Id == recurringItemId);

    public async ValueTask DisposeAsync()
    {
        foreach (Guid recurringItemId in _recurringItemIds)
        {
            await _recurringItemRepository.DeleteAsync(recurringItemId);
        }

        await _testCategories.DisposeAsync();
    }
}
