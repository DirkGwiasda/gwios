using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using CategoryUnderTest = GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models.Category;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models.Category;

/// <summary>
/// Covers the construction of <c>Category</c> and the defaults it assigns.
/// </summary>
public sealed class ConstructorTests
{
    [Fact]
    public void AssignsAVersion7Id()
    {
        CategoryUnderTest category = new() { Name = "Wohnen", Type = CategoryType.Expense };

        Assert.Equal(7, category.Id.Version);
    }

    [Fact]
    public void AssignsADifferentIdToEachCategory()
    {
        CategoryUnderTest firstCategory = new() { Name = "Wohnen", Type = CategoryType.Expense };
        CategoryUnderTest secondCategory = new() { Name = "Wohnen", Type = CategoryType.Expense };

        Assert.NotEqual(firstCategory.Id, secondCategory.Id);
    }

    [Fact]
    public void LeavesParentIdAndColorUnset()
    {
        CategoryUnderTest category = new() { Name = "Wohnen", Type = CategoryType.Expense };

        Assert.Null(category.ParentId);
        Assert.Null(category.Color);
    }
}
