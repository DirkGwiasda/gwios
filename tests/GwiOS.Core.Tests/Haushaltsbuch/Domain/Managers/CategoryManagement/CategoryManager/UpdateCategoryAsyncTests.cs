using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.CategoryManagement.CategoryManager;

/// <summary>
/// Covers <c>CategoryManager.UpdateCategoryAsync</c>.
/// </summary>
public sealed class UpdateCategoryAsyncTests
{
    private readonly CategoryManagerProbe _probe = new();
    private readonly Category _category = new() { Name = "Wohnen", Type = CategoryType.Expense, Color = "#ff4d6d" };

    [Fact]
    public async Task StoresTheChangedCategory_WhenItIsValid()
    {
        await _probe.CategoryRepository.InsertAsync(_category, TestContext.Current.CancellationToken);
        Category changedCategory = new()
        {
            Id = _category.Id,
            Name = "Wohnen & Nebenkosten",
            Type = CategoryType.Expense,
            Color = "#3399ff"
        };

        await _probe.CategoryManager.UpdateCategoryAsync(changedCategory, TestContext.Current.CancellationToken);

        List<Category> storedCategories =
            await _probe.CategoryRepository.GetAllAsync(TestContext.Current.CancellationToken);
        Category storedCategory = Assert.Single(storedCategories);
        Assert.Equal("Wohnen & Nebenkosten", storedCategory.Name);
        Assert.Equal("#3399ff", storedCategory.Color);
    }

    [Fact]
    public async Task ValidatesTheCategoryAgainstTheStoredCategories()
    {
        await _probe.CategoryRepository.InsertAsync(_category, TestContext.Current.CancellationToken);

        await _probe.CategoryManager.UpdateCategoryAsync(_category, TestContext.Current.CancellationToken);

        Assert.Same(_category, Assert.Single(_probe.CategoryValidator.ValidatedCategories));
        Assert.Same(_category, Assert.Single(_probe.CategoryValidator.LastExistingCategories!));
    }

    [Fact]
    public async Task ThrowsCategoryValidationExceptionWithTheValidationErrors_WhenTheCategoryIsInvalid()
    {
        await _probe.CategoryRepository.InsertAsync(_category, TestContext.Current.CancellationToken);
        _probe.CategoryValidator.ValidationErrors = ["First error.", "Second error."];

        CategoryValidationException exception = await Assert.ThrowsAsync<CategoryValidationException>(
            () => _probe.CategoryManager.UpdateCategoryAsync(_category, TestContext.Current.CancellationToken));

        Assert.Equal(_category.Id, exception.CategoryId);
        Assert.Equal(["First error.", "Second error."], exception.ValidationErrors);
    }

    [Fact]
    public async Task ThrowsCategoryNotFoundException_WhenTheCategoryIsNotStored()
    {
        await Assert.ThrowsAsync<CategoryNotFoundException>(
            () => _probe.CategoryManager.UpdateCategoryAsync(_category, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LogsTheUpdateWithTheCategoryId()
    {
        await _probe.CategoryRepository.InsertAsync(_category, TestContext.Current.CancellationToken);

        await _probe.CategoryManager.UpdateCategoryAsync(_category, TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Category updated.", logEntry.Message);
        Assert.Equal(_category.Id.ToString(), logEntry.ContextData["CategoryId"]);
    }
}
