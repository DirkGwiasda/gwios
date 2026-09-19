using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;

/// <summary>
/// Stands in for <see cref="ICategoryValidator"/>. It considers every category valid until a test sets
/// <see cref="ValidationErrors"/>, and records every category it validates together with the existing categories it
/// was given.
/// </summary>
public sealed class CategoryValidatorFake : ICategoryValidator
{
    private readonly List<Category> _validatedCategories = [];

    /// <summary>
    /// The validation errors reported for every category. Empty by default, which makes every category valid.
    /// </summary>
    public List<string> ValidationErrors { get; set; } = [];

    /// <summary>
    /// All categories validated so far, in the order of validation.
    /// </summary>
    public IReadOnlyList<Category> ValidatedCategories
        => _validatedCategories;

    /// <summary>
    /// The <c>existingCategories</c> argument of the most recent call to <see cref="IsValid"/>, or <c>null</c> if it
    /// was not called yet.
    /// </summary>
    public IReadOnlyList<Category>? LastExistingCategories { get; private set; }

    public bool IsValid(
        Category category,
        IReadOnlyList<Category> existingCategories,
        out List<string> validationErrors)
    {
        _validatedCategories.Add(category);
        LastExistingCategories = existingCategories;
        validationErrors = [.. ValidationErrors];
        return validationErrors.Count == 0;
    }
}
