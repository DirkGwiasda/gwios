using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.RecurringItemPostgresRepository;

/// <summary>
/// Covers <c>RecurringItemPostgresRepository.GetAllAsync</c> against the PostgreSQL test database. The database may
/// contain recurring items of other tests, so the tests only look at the recurring items they created themselves.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class GetAllAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly IRecurringItemRepository _recurringItemRepository = database.RecurringItemRepository;
    private readonly TestRecurringItems _testRecurringItems = new(database.CategoryRepository, database.RecurringItemRepository);

    [Fact]
    public async Task ReturnsEveryStoredRecurringItemOnce()
    {
        RecurringItem first = await _testRecurringItems.CreateAsync(TestContext.Current.CancellationToken);
        RecurringItem second = await _testRecurringItems.CreateAsync(TestContext.Current.CancellationToken);
        await _recurringItemRepository.InsertAsync(first, TestContext.Current.CancellationToken);
        await _recurringItemRepository.InsertAsync(second, TestContext.Current.CancellationToken);

        List<RecurringItem> recurringItems =
            await _recurringItemRepository.GetAllAsync(TestContext.Current.CancellationToken);

        Assert.Single(recurringItems, item => item.Id == first.Id);
        Assert.Single(recurringItems, item => item.Id == second.Id);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testRecurringItems.DisposeAsync();
}
