using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement;

internal sealed class CategoryManager(
    ICategoryRepository categoryRepository,
    ICategoryValidator categoryValidator,
    ILogger<CategoryManager> logger) : ICategoryManager
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly ICategoryValidator _categoryValidator = categoryValidator;
    private readonly ILogger<CategoryManager> _logger = logger;

    public async Task<Category> CreateCategoryAsync(Category category, CancellationToken cancellationToken = default)
    {
        List<Category> existingCategories = await _categoryRepository.GetAllAsync(cancellationToken);
        ThrowIfInvalid(category, existingCategories);
        await _categoryRepository.InsertAsync(category, cancellationToken);
        _logger.LogInformation("Category created.", CreateCategoryIdData(category.Id));
        return category;
    }

    public async Task UpdateCategoryAsync(Category category, CancellationToken cancellationToken = default)
    {
        List<Category> existingCategories = await _categoryRepository.GetAllAsync(cancellationToken);
        ThrowIfInvalid(category, existingCategories);
        await _categoryRepository.UpdateAsync(category, cancellationToken);
        _logger.LogInformation("Category updated.", CreateCategoryIdData(category.Id));
    }

    public async Task<List<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
        => await _categoryRepository.GetAllAsync(cancellationToken);

    public async Task DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        List<Category> existingCategories = await _categoryRepository.GetAllAsync(cancellationToken);
        if (existingCategories.Any(existing => existing.ParentId == categoryId))
        {
            throw new CategoryHasChildrenException(categoryId);
        }

        await _categoryRepository.DeleteAsync(categoryId, cancellationToken);
        _logger.LogInformation("Category deleted.", CreateCategoryIdData(categoryId));
    }

    private void ThrowIfInvalid(Category category, List<Category> existingCategories)
    {
        if (!_categoryValidator.IsValid(category, existingCategories, out List<string> validationErrors))
        {
            throw new CategoryValidationException(category.Id, validationErrors);
        }
    }

    private static Dictionary<string, string> CreateCategoryIdData(Guid categoryId)
        => new() { ["CategoryId"] = categoryId.ToString() };
}
