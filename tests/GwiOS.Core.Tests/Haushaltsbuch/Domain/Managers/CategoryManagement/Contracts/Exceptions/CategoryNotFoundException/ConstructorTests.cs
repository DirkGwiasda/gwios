using CategoryNotFoundExceptionUnderTest =
    GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions.CategoryNotFoundException;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions.CategoryNotFoundException;

/// <summary>
/// Covers the constructor of <c>CategoryNotFoundException</c>.
/// </summary>
public sealed class ConstructorTests
{
    private readonly Guid _categoryId = Guid.CreateVersion7();

    [Fact]
    public void KeepsTheCategoryId()
    {
        CategoryNotFoundExceptionUnderTest exception = new(_categoryId);

        Assert.Equal(_categoryId, exception.CategoryId);
    }

    [Fact]
    public void NamesTheCategoryIdInTheMessage()
    {
        CategoryNotFoundExceptionUnderTest exception = new(_categoryId);

        Assert.Equal($"No category with ID '{_categoryId}' is stored.", exception.Message);
    }
}
