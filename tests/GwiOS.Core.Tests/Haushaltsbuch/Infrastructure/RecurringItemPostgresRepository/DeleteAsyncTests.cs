using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.RecurringItemPostgresRepository;

/// <summary>
/// Covers <c>RecurringItemPostgresRepository.DeleteAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class DeleteAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly IRecurringItemRepository _recurringItemRepository = database.RecurringItemRepository;
    private readonly TestRecurringItems _testRecurringItems = new(database.CategoryRepository, database.RecurringItemRepository);

    [Fact]
    public async Task DeletesOnlyTheRecurringItemWithTheGivenId()
    {
        RecurringItem toDelete = await _testRecurringItems.CreateAsync(TestContext.Current.CancellationToken);
        RecurringItem toKeep = await _testRecurringItems.CreateAsync(TestContext.Current.CancellationToken);
        await _recurringItemRepository.InsertAsync(toDelete, TestContext.Current.CancellationToken);
        await _recurringItemRepository.InsertAsync(toKeep, TestContext.Current.CancellationToken);

        await _recurringItemRepository.DeleteAsync(toDelete.Id, TestContext.Current.CancellationToken);

        Assert.Null(await _testRecurringItems.FindStoredAsync(toDelete.Id));
        Assert.NotNull(await _testRecurringItems.FindStoredAsync(toKeep.Id));
    }

    [Fact]
    public async Task Succeeds_WhenTheRecurringItemIsNotStored()
    {
        Exception? exception = await Record.ExceptionAsync(
            () => _recurringItemRepository.DeleteAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken));

        Assert.Null(exception);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testRecurringItems.DisposeAsync();
}
