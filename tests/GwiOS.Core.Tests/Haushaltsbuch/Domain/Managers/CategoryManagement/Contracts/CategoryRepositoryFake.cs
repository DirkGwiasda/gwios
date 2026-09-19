using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;

/// <summary>
/// Stands in for <see cref="ICategoryRepository"/> by keeping categories in memory. It orders, updates and deletes
/// like the real repository.
/// </summary>
public sealed class CategoryRepositoryFake : ICategoryRepository
{
    private readonly List<Category> _categories = [];

    public Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task InsertAsync(Category category, CancellationToken cancellationToken = default)
    {
        _categories.Add(category);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        int index = _categories.FindIndex(stored => stored.Id == category.Id);
        if (index < 0)
        {
            throw new CategoryNotFoundException(category.Id);
        }

        _categories[index] = category;
        return Task.CompletedTask;
    }

    public Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_categories.OrderBy(category => category.Type).ThenBy(category => category.Name).ToList());

    public Task DeleteAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        _categories.RemoveAll(category => category.Id == categoryId);
        return Task.CompletedTask;
    }
}
