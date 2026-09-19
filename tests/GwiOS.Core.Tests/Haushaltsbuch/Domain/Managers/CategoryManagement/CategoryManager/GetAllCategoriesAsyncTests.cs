using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.CategoryManagement.CategoryManager;

/// <summary>
/// Covers <c>CategoryManager.GetAllCategoriesAsync</c>.
/// </summary>
public sealed class GetAllCategoriesAsyncTests
{
    private readonly CategoryManagerProbe _probe = new();

    [Fact]
    public async Task ReturnsTheCategoriesOfTheRepositoryInItsOrder()
    {
        Category expenseCategory = new() { Name = "Wohnen", Type = CategoryType.Expense, Color = "#ff4d6d" };
        Category incomeCategory = new() { Name = "Gehalt", Type = CategoryType.Income, Color = "#00ffa3" };
        await _probe.CategoryRepository.InsertAsync(expenseCategory, TestContext.Current.CancellationToken);
        await _probe.CategoryRepository.InsertAsync(incomeCategory, TestContext.Current.CancellationToken);

        List<Category> categories =
            await _probe.CategoryManager.GetAllCategoriesAsync(TestContext.Current.CancellationToken);

        Assert.Equal([incomeCategory, expenseCategory], categories);
    }

    [Fact]
    public async Task ReturnsAnEmptyList_WhenNoCategoriesAreStored()
    {
        List<Category> categories =
            await _probe.CategoryManager.GetAllCategoriesAsync(TestContext.Current.CancellationToken);

        Assert.Empty(categories);
    }
}
