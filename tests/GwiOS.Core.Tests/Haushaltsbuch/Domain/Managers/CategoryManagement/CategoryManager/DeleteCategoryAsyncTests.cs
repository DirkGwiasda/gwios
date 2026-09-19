using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.CategoryManagement.CategoryManager;

/// <summary>
/// Covers <c>CategoryManager.DeleteCategoryAsync</c>.
/// </summary>
public sealed class DeleteCategoryAsyncTests
{
    private readonly CategoryManagerProbe _probe = new();
    private readonly Category _wohnen = new() { Name = "Wohnen", Type = CategoryType.Expense, Color = "#ff4d6d" };
    private readonly Category _freizeit = new() { Name = "Freizeit", Type = CategoryType.Expense, Color = "#3399ff" };

    [Fact]
    public async Task DeletesOnlyTheCategoryWithTheGivenId()
    {
        await _probe.CategoryRepository.InsertAsync(_wohnen, TestContext.Current.CancellationToken);
        await _probe.CategoryRepository.InsertAsync(_freizeit, TestContext.Current.CancellationToken);

        await _probe.CategoryManager.DeleteCategoryAsync(_wohnen.Id, TestContext.Current.CancellationToken);

        List<Category> storedCategories =
            await _probe.CategoryRepository.GetAllAsync(TestContext.Current.CancellationToken);
        Assert.Same(_freizeit, Assert.Single(storedCategories));
    }

    [Fact]
    public async Task ThrowsCategoryHasChildrenException_WhenAnotherCategoryHasItAsItsParent()
    {
        await _probe.CategoryRepository.InsertAsync(_wohnen, TestContext.Current.CancellationToken);
        Category miete = new()
        {
            Name = "Miete", Type = CategoryType.Expense, ParentId = _wohnen.Id
        };
        await _probe.CategoryRepository.InsertAsync(miete, TestContext.Current.CancellationToken);

        CategoryHasChildrenException exception = await Assert.ThrowsAsync<CategoryHasChildrenException>(
            () => _probe.CategoryManager.DeleteCategoryAsync(_wohnen.Id, TestContext.Current.CancellationToken));

        Assert.Equal(_wohnen.Id, exception.CategoryId);
    }

    [Fact]
    public async Task DoesNotDeleteTheCategory_WhenItHasChildren()
    {
        await _probe.CategoryRepository.InsertAsync(_wohnen, TestContext.Current.CancellationToken);
        Category miete = new()
        {
            Name = "Miete", Type = CategoryType.Expense, ParentId = _wohnen.Id
        };
        await _probe.CategoryRepository.InsertAsync(miete, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<CategoryHasChildrenException>(
            () => _probe.CategoryManager.DeleteCategoryAsync(_wohnen.Id, TestContext.Current.CancellationToken));

        List<Category> storedCategories =
            await _probe.CategoryRepository.GetAllAsync(TestContext.Current.CancellationToken);
        Assert.Contains(storedCategories, category => category.Id == _wohnen.Id);
    }

    [Fact]
    public async Task Succeeds_WhenTheCategoryIsNotStored()
    {
        Exception? exception = await Record.ExceptionAsync(
            () => _probe.CategoryManager.DeleteCategoryAsync(_wohnen.Id, TestContext.Current.CancellationToken));

        Assert.Null(exception);
    }

    [Fact]
    public async Task LogsTheDeletionWithTheCategoryId()
    {
        await _probe.CategoryRepository.InsertAsync(_wohnen, TestContext.Current.CancellationToken);

        await _probe.CategoryManager.DeleteCategoryAsync(_wohnen.Id, TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Category deleted.", logEntry.Message);
        Assert.Equal(_wohnen.Id.ToString(), logEntry.ContextData["CategoryId"]);
    }
}
