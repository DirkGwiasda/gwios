using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.CategoryPostgresRepository;

/// <summary>
/// Covers <c>CategoryPostgresRepository.GetAllAsync</c> against the PostgreSQL test database. The database may
/// contain categories of other tests, so the tests only look at the categories they created themselves.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class GetAllAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly ICategoryRepository _categoryRepository = database.CategoryRepository;
    private readonly TestCategories _testCategories = new(database.CategoryRepository);

    [Fact]
    public async Task ReturnsEveryStoredCategoryOnce()
    {
        Category firstCategory = _testCategories.Create();
        Category secondCategory = _testCategories.Create();
        await _categoryRepository.InsertAsync(firstCategory, TestContext.Current.CancellationToken);
        await _categoryRepository.InsertAsync(secondCategory, TestContext.Current.CancellationToken);

        List<Category> categories = await _categoryRepository.GetAllAsync(TestContext.Current.CancellationToken);

        Assert.Single(categories, category => category.Id == firstCategory.Id);
        Assert.Single(categories, category => category.Id == secondCategory.Id);
    }

    [Fact]
    public async Task ReturnsIncomeCategoriesBeforeExpenseCategoriesOfTheSameName()
    {
        string sharedName = _testCategories.CreateUniqueName();
        Category expenseCategory = new() { Name = sharedName, Type = CategoryType.Expense, Color = "#ff4d6d" };
        Category incomeCategory = new() { Name = sharedName, Type = CategoryType.Income, Color = "#00ffa3" };
        await _categoryRepository.InsertAsync(expenseCategory, TestContext.Current.CancellationToken);
        await _categoryRepository.InsertAsync(incomeCategory, TestContext.Current.CancellationToken);
        try
        {
            List<Category> categories = await _categoryRepository.GetAllAsync(TestContext.Current.CancellationToken);

            List<Category> matching = [.. categories.Where(category => category.Name == sharedName)];
            Assert.Equal([incomeCategory.Id, expenseCategory.Id], matching.Select(category => category.Id));
        }
        finally
        {
            await _categoryRepository.DeleteAsync(expenseCategory.Id, TestContext.Current.CancellationToken);
            await _categoryRepository.DeleteAsync(incomeCategory.Id, TestContext.Current.CancellationToken);
        }
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testCategories.DisposeAsync();
}
