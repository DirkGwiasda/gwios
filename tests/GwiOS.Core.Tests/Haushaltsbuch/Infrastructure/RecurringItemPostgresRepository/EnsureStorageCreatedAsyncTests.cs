using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.RecurringItemPostgresRepository;

/// <summary>
/// Covers <c>RecurringItemPostgresRepository.EnsureStorageCreatedAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class EnsureStorageCreatedAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly IRecurringItemRepository _recurringItemRepository = database.RecurringItemRepository;
    private readonly TestRecurringItems _testRecurringItems = new(database.CategoryRepository, database.RecurringItemRepository);

    [Fact]
    public async Task KeepsExistingRecurringItems_WhenTheTablesAlreadyExist()
    {
        RecurringItem recurringItem = await _testRecurringItems.CreateAsync(TestContext.Current.CancellationToken);
        await _recurringItemRepository.InsertAsync(recurringItem, TestContext.Current.CancellationToken);

        await _recurringItemRepository.EnsureStorageCreatedAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(await _testRecurringItems.FindStoredAsync(recurringItem.Id));
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testRecurringItems.DisposeAsync();
}
