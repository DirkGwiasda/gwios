using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.CategoryPostgresRepository;

/// <summary>
/// Covers <c>CategoryPostgresRepository.EnsureStorageCreatedAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class EnsureStorageCreatedAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly ICategoryRepository _categoryRepository = database.CategoryRepository;
    private readonly TestCategories _testCategories = new(database.CategoryRepository);

    [Fact]
    public async Task KeepsExistingCategories_WhenTheTablesAlreadyExist()
    {
        Category category = _testCategories.Create();
        await _categoryRepository.InsertAsync(category, TestContext.Current.CancellationToken);

        await _categoryRepository.EnsureStorageCreatedAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(await _testCategories.FindStoredAsync(category.Id));
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testCategories.DisposeAsync();
}
