using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.CategoryPostgresRepository;

/// <summary>
/// Covers <c>CategoryPostgresRepository.InsertAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class InsertAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly ICategoryRepository _categoryRepository = database.CategoryRepository;
    private readonly TestCategories _testCategories = new(database.CategoryRepository);

    [Fact]
    public async Task StoresAllPropertiesOfARootCategory()
    {
        Category category = _testCategories.Create();

        await _categoryRepository.InsertAsync(category, TestContext.Current.CancellationToken);

        Category? storedCategory = await _testCategories.FindStoredAsync(category.Id);
        Assert.NotNull(storedCategory);
        Assert.Equal(category.Name, storedCategory.Name);
        Assert.Equal(category.Type, storedCategory.Type);
        Assert.Null(storedCategory.ParentId);
        Assert.Equal(category.Color, storedCategory.Color);
    }

    [Fact]
    public async Task StoresASubCategoryWithoutAColorAndWithTheParentId()
    {
        Category parent = _testCategories.Create();
        await _categoryRepository.InsertAsync(parent, TestContext.Current.CancellationToken);
        Category child = _testCategories.CreateChild(parent);

        await _categoryRepository.InsertAsync(child, TestContext.Current.CancellationToken);

        Category? storedChild = await _testCategories.FindStoredAsync(child.Id);
        Assert.NotNull(storedChild);
        Assert.Equal(parent.Id, storedChild.ParentId);
        Assert.Null(storedChild.Color);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testCategories.DisposeAsync();
}
