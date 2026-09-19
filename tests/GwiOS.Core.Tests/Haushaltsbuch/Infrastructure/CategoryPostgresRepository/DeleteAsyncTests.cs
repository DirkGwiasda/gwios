using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.CategoryPostgresRepository;

/// <summary>
/// Covers <c>CategoryPostgresRepository.DeleteAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class DeleteAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly ICategoryRepository _categoryRepository = database.CategoryRepository;
    private readonly TestCategories _testCategories = new(database.CategoryRepository);

    [Fact]
    public async Task DeletesOnlyTheCategoryWithTheGivenId()
    {
        Category categoryToDelete = _testCategories.Create();
        Category categoryToKeep = _testCategories.Create();
        await _categoryRepository.InsertAsync(categoryToDelete, TestContext.Current.CancellationToken);
        await _categoryRepository.InsertAsync(categoryToKeep, TestContext.Current.CancellationToken);

        await _categoryRepository.DeleteAsync(categoryToDelete.Id, TestContext.Current.CancellationToken);

        Assert.Null(await _testCategories.FindStoredAsync(categoryToDelete.Id));
        Assert.NotNull(await _testCategories.FindStoredAsync(categoryToKeep.Id));
    }

    [Fact]
    public async Task Succeeds_WhenTheCategoryIsNotStored()
    {
        Exception? exception = await Record.ExceptionAsync(
            () => _categoryRepository.DeleteAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken));

        Assert.Null(exception);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testCategories.DisposeAsync();
}
