using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.CategoryPostgresRepository;

/// <summary>
/// Creates categories for the tests against the PostgreSQL test database and deletes every category it created from
/// the database when it is disposed, children before their parents (a foreign key forbids the other order). The
/// database may contain categories of other tests, so tests only look at the categories they created themselves.
/// </summary>
public sealed class TestCategories(ICategoryRepository categoryRepository) : IAsyncDisposable
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly List<Guid> _categoryIds = [];

    /// <summary>
    /// Returns a new, not yet stored, root category with a unique name and a color.
    /// </summary>
    public Category Create()
    {
        Category category = new() { Name = CreateUniqueName(), Type = CategoryType.Expense, Color = "#ff4d6d" };
        _categoryIds.Add(category.Id);
        return category;
    }

    /// <summary>
    /// Returns a new, not yet stored, sub-category of the given parent, with a unique name, the parent's type and
    /// without a color.
    /// </summary>
    public Category CreateChild(Category parent)
    {
        Category category = new() { Name = CreateUniqueName(), Type = parent.Type, ParentId = parent.Id };
        _categoryIds.Add(category.Id);
        return category;
    }

    /// <summary>
    /// Returns a name that no other category uses, prefixed so that leftovers are recognizable as test data.
    /// </summary>
    public string CreateUniqueName()
        => $"test-{Guid.NewGuid():N}";

    /// <summary>
    /// Returns the stored category with the given ID, or <c>null</c> if no such category is stored.
    /// </summary>
    public async Task<Category?> FindStoredAsync(Guid categoryId)
        => (await _categoryRepository.GetAllAsync()).SingleOrDefault(category => category.Id == categoryId);

    public async ValueTask DisposeAsync()
    {
        for (int index = _categoryIds.Count - 1; index >= 0; index--)
        {
            await _categoryRepository.DeleteAsync(_categoryIds[index]);
        }
    }
}
