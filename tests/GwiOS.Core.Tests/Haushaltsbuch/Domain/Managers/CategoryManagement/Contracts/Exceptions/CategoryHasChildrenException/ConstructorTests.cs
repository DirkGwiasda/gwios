using CategoryHasChildrenExceptionUnderTest =
    GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions.CategoryHasChildrenException;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions.CategoryHasChildrenException;

/// <summary>
/// Covers the constructor of <c>CategoryHasChildrenException</c>.
/// </summary>
public sealed class ConstructorTests
{
    private readonly Guid _categoryId = Guid.CreateVersion7();

    [Fact]
    public void KeepsTheCategoryId()
    {
        CategoryHasChildrenExceptionUnderTest exception = new(_categoryId);

        Assert.Equal(_categoryId, exception.CategoryId);
    }

    [Fact]
    public void NamesTheCategoryIdInTheMessage()
    {
        CategoryHasChildrenExceptionUnderTest exception = new(_categoryId);

        Assert.Equal(
            $"Category '{_categoryId}' cannot be deleted because it still has sub-categories.",
            exception.Message);
    }
}
