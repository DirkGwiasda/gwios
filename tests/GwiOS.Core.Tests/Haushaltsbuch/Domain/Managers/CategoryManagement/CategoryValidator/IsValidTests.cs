using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using CategoryValidatorUnderTest = GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.CategoryValidator;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.CategoryManagement.CategoryValidator;

/// <summary>
/// Covers <c>CategoryValidator.IsValid</c>.
/// </summary>
public sealed class IsValidTests
{
    private readonly LoggerFake<CategoryValidatorUnderTest> _logger = new();
    private readonly CategoryValidatorUnderTest _categoryValidator;

    public IsValidTests()
    {
        _categoryValidator = new CategoryValidatorUnderTest(_logger);
    }

    [Fact]
    public void ReturnsTrueWithoutValidationErrors_WhenARootCategoryIsValid()
    {
        Category category = new() { Name = "Wohnen", Type = CategoryType.Expense, Color = "#ff4d6d" };

        bool isValid = _categoryValidator.IsValid(category, [], out List<string> validationErrors);

        Assert.True(isValid);
        Assert.Empty(validationErrors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnsFalseWithAnError_WhenTheNameIsEmpty(string name)
    {
        Category category = new() { Name = name, Type = CategoryType.Expense, Color = "#ff4d6d" };

        bool isValid = _categoryValidator.IsValid(category, [], out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Contains("The name must not be empty.", validationErrors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ff4d6d")]
    [InlineData("#ff4d6")]
    [InlineData("#gggggg")]
    public void ReturnsFalseWithAnError_WhenARootCategoryHasNoValidColor(string? color)
    {
        Category category = new() { Name = "Wohnen", Type = CategoryType.Expense, Color = color };

        bool isValid = _categoryValidator.IsValid(category, [], out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Contains(
            "The color of a root category must be a valid hex code (e.g. '#3399ff').",
            validationErrors);
    }

    [Fact]
    public void ReturnsFalseWithAnError_WhenASubCategoryHasItsOwnColor()
    {
        Category parent = new() { Name = "Wohnen", Type = CategoryType.Expense, Color = "#ff4d6d" };
        Category category = new()
        {
            Name = "Miete", Type = CategoryType.Expense, ParentId = parent.Id, Color = "#3399ff"
        };

        bool isValid = _categoryValidator.IsValid(category, [parent], out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Contains("A sub-category must not have its own color.", validationErrors);
    }

    [Fact]
    public void ReturnsTrueWithoutValidationErrors_WhenASubCategoryIsValid()
    {
        Category parent = new() { Name = "Wohnen", Type = CategoryType.Expense, Color = "#ff4d6d" };
        Category category = new() { Name = "Miete", Type = CategoryType.Expense, ParentId = parent.Id };

        bool isValid = _categoryValidator.IsValid(category, [parent], out List<string> validationErrors);

        Assert.True(isValid);
        Assert.Empty(validationErrors);
    }

    [Fact]
    public void ReturnsFalseWithAnError_WhenACategoryIsItsOwnParent()
    {
        Category category = new() { Name = "Wohnen", Type = CategoryType.Expense, Color = "#ff4d6d" };
        category.ParentId = category.Id;

        bool isValid = _categoryValidator.IsValid(category, [category], out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Contains("A category cannot be its own parent category.", validationErrors);
    }

    [Fact]
    public void ReturnsFalseWithAnError_WhenTheParentCategoryIsNotStored()
    {
        Category category = new()
        {
            Name = "Miete", Type = CategoryType.Expense, ParentId = Guid.CreateVersion7()
        };

        bool isValid = _categoryValidator.IsValid(category, [], out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Contains("The parent category is not stored.", validationErrors);
    }

    [Fact]
    public void ReturnsFalseWithAnError_WhenTheTypeDoesNotMatchTheParentType()
    {
        Category parent = new() { Name = "Gehalt", Type = CategoryType.Income, Color = "#00ffa3" };
        Category category = new()
        {
            Name = "Miete", Type = CategoryType.Expense, ParentId = parent.Id
        };

        bool isValid = _categoryValidator.IsValid(category, [parent], out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Contains("The type must match the parent category's type.", validationErrors);
    }

    [Fact]
    public void ReturnsFalseWithAnError_WhenTheChosenParentIsADescendantOfTheCategory()
    {
        Category root = new() { Name = "Wohnen", Type = CategoryType.Expense, Color = "#ff4d6d" };
        Category child = new() { Name = "Miete", Type = CategoryType.Expense, ParentId = root.Id };
        Category rootMovedUnderChild = new()
        {
            Id = root.Id, Name = "Wohnen", Type = CategoryType.Expense, ParentId = child.Id
        };

        bool isValid = _categoryValidator.IsValid(
            rootMovedUnderChild,
            [root, child],
            out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Contains("The parent category must not be a sub-category of this category.", validationErrors);
    }

    [Fact]
    public void ReturnsTrueWithoutValidationErrors_WhenTheCategoryIsExactlyFiveLevelsDeep()
    {
        Category level1 = new() { Name = "L1", Type = CategoryType.Expense, Color = "#ff4d6d" };
        Category level2 = new() { Name = "L2", Type = CategoryType.Expense, ParentId = level1.Id };
        Category level3 = new() { Name = "L3", Type = CategoryType.Expense, ParentId = level2.Id };
        Category level4 = new() { Name = "L4", Type = CategoryType.Expense, ParentId = level3.Id };
        Category level5 = new() { Name = "L5", Type = CategoryType.Expense, ParentId = level4.Id };

        bool isValid = _categoryValidator.IsValid(
            level5,
            [level1, level2, level3, level4],
            out List<string> validationErrors);

        Assert.True(isValid);
        Assert.Empty(validationErrors);
    }

    [Fact]
    public void ReturnsFalseWithAnError_WhenTheCategoryWouldBeSixLevelsDeep()
    {
        Category level1 = new() { Name = "L1", Type = CategoryType.Expense, Color = "#ff4d6d" };
        Category level2 = new() { Name = "L2", Type = CategoryType.Expense, ParentId = level1.Id };
        Category level3 = new() { Name = "L3", Type = CategoryType.Expense, ParentId = level2.Id };
        Category level4 = new() { Name = "L4", Type = CategoryType.Expense, ParentId = level3.Id };
        Category level5 = new() { Name = "L5", Type = CategoryType.Expense, ParentId = level4.Id };
        Category level6 = new() { Name = "L6", Type = CategoryType.Expense, ParentId = level5.Id };

        bool isValid = _categoryValidator.IsValid(
            level6,
            [level1, level2, level3, level4, level5],
            out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Contains("Categories can be nested at most 5 levels deep.", validationErrors);
    }

    [Fact]
    public void ReturnsFalseWithAnError_WhenAnotherRootCategoryHasTheSameNameCaseInsensitively()
    {
        Category existing = new() { Name = "Wohnen", Type = CategoryType.Expense, Color = "#ff4d6d" };
        Category category = new() { Name = "wohnen", Type = CategoryType.Expense, Color = "#3399ff" };

        bool isValid = _categoryValidator.IsValid(category, [existing], out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Contains("Another category at the same level already uses this name.", validationErrors);
    }

    [Fact]
    public void ReturnsTrueWithoutValidationErrors_WhenTheSameNameIsUsedUnderADifferentParent()
    {
        Category firstParent = new() { Name = "Wohnen", Type = CategoryType.Expense, Color = "#ff4d6d" };
        Category secondParent = new() { Name = "Freizeit", Type = CategoryType.Expense, Color = "#3399ff" };
        Category existingChild = new()
        {
            Name = "Sonstiges", Type = CategoryType.Expense, ParentId = firstParent.Id
        };
        Category category = new()
        {
            Name = "Sonstiges", Type = CategoryType.Expense, ParentId = secondParent.Id
        };

        bool isValid = _categoryValidator.IsValid(
            category,
            [firstParent, secondParent, existingChild],
            out List<string> validationErrors);

        Assert.True(isValid);
        Assert.Empty(validationErrors);
    }

    [Fact]
    public void ReturnsTrueWithoutValidationErrors_WhenUpdatingACategoryWithoutChangingItsName()
    {
        Category category = new() { Name = "Wohnen", Type = CategoryType.Expense, Color = "#ff4d6d" };

        bool isValid = _categoryValidator.IsValid(category, [category], out List<string> validationErrors);

        Assert.True(isValid);
        Assert.Empty(validationErrors);
    }

    [Fact]
    public void LogsAWarningWithTheCategoryIdAndTheErrors_WhenTheCategoryIsInvalid()
    {
        Category category = new() { Name = "", Type = CategoryType.Expense };

        _categoryValidator.IsValid(category, [], out _);

        LogEntry logEntry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Warning, logEntry.LogLevel);
        Assert.Equal(category.Id.ToString(), logEntry.ContextData["CategoryId"]);
        Assert.Contains("The name must not be empty.", logEntry.ContextData["ValidationErrors"]);
    }

    [Fact]
    public void LogsNothing_WhenTheCategoryIsValid()
    {
        Category category = new() { Name = "Wohnen", Type = CategoryType.Expense, Color = "#ff4d6d" };

        _categoryValidator.IsValid(category, [], out _);

        Assert.Empty(_logger.Entries);
    }
}
