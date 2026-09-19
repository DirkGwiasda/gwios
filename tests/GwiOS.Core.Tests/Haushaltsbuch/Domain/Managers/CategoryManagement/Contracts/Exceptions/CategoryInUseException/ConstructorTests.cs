using CategoryInUseExceptionUnderTest =
    GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions.CategoryInUseException;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions.CategoryInUseException;

/// <summary>
/// Covers the constructor of <c>CategoryInUseException</c>.
/// </summary>
public sealed class ConstructorTests
{
    private readonly Guid _categoryId = Guid.CreateVersion7();

    [Fact]
    public void KeepsTheCategoryId()
    {
        CategoryInUseExceptionUnderTest exception = new(_categoryId);

        Assert.Equal(_categoryId, exception.CategoryId);
    }

    [Fact]
    public void NamesTheCategoryIdInTheMessage()
    {
        CategoryInUseExceptionUnderTest exception = new(_categoryId);

        Assert.Equal(
            $"Category '{_categoryId}' cannot be deleted because it is still used by a booking or recurring item.",
            exception.Message);
    }
}
