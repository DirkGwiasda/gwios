using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.CategoryPostgresRepository;

/// <summary>
/// Covers <c>CategoryPostgresRepository.UpdateAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class UpdateAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly PostgresTestDatabase _database = database;
    private readonly ICategoryRepository _categoryRepository = database.CategoryRepository;
    private readonly TestCategories _testCategories = new(database.CategoryRepository);

    [Fact]
    public async Task OverwritesAllChangeablePropertiesOfTheStoredCategory()
    {
        Category category = _testCategories.Create();
        await _categoryRepository.InsertAsync(category, TestContext.Current.CancellationToken);
        Category changedCategory = new()
        {
            Id = category.Id,
            Name = _testCategories.CreateUniqueName(),
            Type = category.Type,
            Color = "#3399ff"
        };

        await _categoryRepository.UpdateAsync(changedCategory, TestContext.Current.CancellationToken);

        Category? storedCategory = await _testCategories.FindStoredAsync(category.Id);
        Assert.NotNull(storedCategory);
        Assert.Equal(changedCategory.Name, storedCategory.Name);
        Assert.Equal(changedCategory.Color, storedCategory.Color);
    }

    [Fact]
    public async Task MovesTheCategoryUnderANewParent()
    {
        Category firstParent = _testCategories.Create();
        Category secondParent = _testCategories.Create();
        await _categoryRepository.InsertAsync(firstParent, TestContext.Current.CancellationToken);
        await _categoryRepository.InsertAsync(secondParent, TestContext.Current.CancellationToken);
        Category child = _testCategories.CreateChild(firstParent);
        await _categoryRepository.InsertAsync(child, TestContext.Current.CancellationToken);
        Category movedChild = new() { Id = child.Id, Name = child.Name, Type = child.Type, ParentId = secondParent.Id };

        await _categoryRepository.UpdateAsync(movedChild, TestContext.Current.CancellationToken);

        Assert.Equal(secondParent.Id, (await _testCategories.FindStoredAsync(child.Id))?.ParentId);
    }

    [Fact]
    public async Task ThrowsCategoryNotFoundException_WhenTheCategoryIsNotStored()
    {
        Category category = _testCategories.Create();

        CategoryNotFoundException exception = await Assert.ThrowsAsync<CategoryNotFoundException>(
            () => _categoryRepository.UpdateAsync(category, TestContext.Current.CancellationToken));

        Assert.Equal(category.Id, exception.CategoryId);
    }

    [Fact]
    public async Task LogsAWarningWithTheCategoryId_WhenTheCategoryIsNotStored()
    {
        Category category = _testCategories.Create();

        await Assert.ThrowsAsync<CategoryNotFoundException>(
            () => _categoryRepository.UpdateAsync(category, TestContext.Current.CancellationToken));

        Assert.Contains(
            _database.CategoryRepositoryLogger.Entries,
            logEntry => (logEntry.LogLevel == LogLevel.Warning)
                && (logEntry.ContextData.GetValueOrDefault("CategoryId") == category.Id.ToString()));
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testCategories.DisposeAsync();
}
