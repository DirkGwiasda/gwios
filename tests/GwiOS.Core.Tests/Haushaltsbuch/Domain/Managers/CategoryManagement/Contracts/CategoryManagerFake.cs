using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;

/// <summary>
/// Stands in for <see cref="ICategoryManager"/> for the workflow tests. It keeps categories in memory and enforces
/// the same has-children delete guard as the real manager, but does not validate.
/// </summary>
public sealed class CategoryManagerFake : ICategoryManager
{
    private readonly List<Category> _categories = [];

    /// <summary>
    /// The IDs of the categories deleted so far, in call order.
    /// </summary>
    public List<Guid> DeletedCategoryIds { get; } = [];

    /// <summary>
    /// Stores a category directly, as if it had been created before the test.
    /// </summary>
    public void Add(Category category)
        => _categories.Add(category);

    public Task<Category> CreateCategoryAsync(Category category, CancellationToken cancellationToken = default)
    {
        _categories.Add(category);
        return Task.FromResult(category);
    }

    public Task UpdateCategoryAsync(Category category, CancellationToken cancellationToken = default)
    {
        int index = _categories.FindIndex(stored => stored.Id == category.Id);
        _categories[index] = category;
        return Task.CompletedTask;
    }

    public Task<List<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_categories.OrderBy(category => category.Type).ThenBy(category => category.Name).ToList());

    public Task DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        if (_categories.Any(category => category.ParentId == categoryId))
        {
            throw new CategoryHasChildrenException(categoryId);
        }

        _categories.RemoveAll(category => category.Id == categoryId);
        DeletedCategoryIds.Add(categoryId);
        return Task.CompletedTask;
    }
}
