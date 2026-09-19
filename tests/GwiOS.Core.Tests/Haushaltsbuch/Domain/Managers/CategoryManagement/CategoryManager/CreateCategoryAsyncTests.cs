using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.CategoryManagement.CategoryManager;

/// <summary>
/// Covers <c>CategoryManager.CreateCategoryAsync</c>.
/// </summary>
public sealed class CreateCategoryAsyncTests
{
    private readonly CategoryManagerProbe _probe = new();
    private readonly Category _category = new() { Name = "Wohnen", Type = CategoryType.Expense, Color = "#ff4d6d" };

    [Fact]
    public async Task StoresTheCategory_WhenItIsValid()
    {
        await _probe.CategoryManager.CreateCategoryAsync(_category, TestContext.Current.CancellationToken);

        List<Category> storedCategories =
            await _probe.CategoryRepository.GetAllAsync(TestContext.Current.CancellationToken);
        Assert.Same(_category, Assert.Single(storedCategories));
    }

    [Fact]
    public async Task ReturnsTheStoredCategory()
    {
        Category createdCategory =
            await _probe.CategoryManager.CreateCategoryAsync(_category, TestContext.Current.CancellationToken);

        Assert.Same(_category, createdCategory);
    }

    [Fact]
    public async Task ValidatesTheCategoryAgainstTheStoredCategories()
    {
        Category existingCategory = new() { Name = "Freizeit", Type = CategoryType.Expense, Color = "#3399ff" };
        await _probe.CategoryRepository.InsertAsync(existingCategory, TestContext.Current.CancellationToken);

        await _probe.CategoryManager.CreateCategoryAsync(_category, TestContext.Current.CancellationToken);

        Assert.Same(_category, Assert.Single(_probe.CategoryValidator.ValidatedCategories));
        Assert.Same(existingCategory, Assert.Single(_probe.CategoryValidator.LastExistingCategories!));
    }

    [Fact]
    public async Task ThrowsCategoryValidationExceptionWithTheValidationErrors_WhenTheCategoryIsInvalid()
    {
        _probe.CategoryValidator.ValidationErrors = ["First error.", "Second error."];

        CategoryValidationException exception = await Assert.ThrowsAsync<CategoryValidationException>(
            () => _probe.CategoryManager.CreateCategoryAsync(_category, TestContext.Current.CancellationToken));

        Assert.Equal(_category.Id, exception.CategoryId);
        Assert.Equal(["First error.", "Second error."], exception.ValidationErrors);
    }

    [Fact]
    public async Task DoesNotStoreTheCategory_WhenItIsInvalid()
    {
        _probe.CategoryValidator.ValidationErrors = ["Error."];

        await Assert.ThrowsAsync<CategoryValidationException>(
            () => _probe.CategoryManager.CreateCategoryAsync(_category, TestContext.Current.CancellationToken));

        Assert.Empty(await _probe.CategoryRepository.GetAllAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LogsTheCreationWithTheCategoryId()
    {
        await _probe.CategoryManager.CreateCategoryAsync(_category, TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Category created.", logEntry.Message);
        Assert.Equal(_category.Id.ToString(), logEntry.ContextData["CategoryId"]);
    }
}
