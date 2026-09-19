using CategoryValidationExceptionUnderTest =
    GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions.CategoryValidationException;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions.CategoryValidationException;

/// <summary>
/// Covers the constructor of <c>CategoryValidationException</c>.
/// </summary>
public sealed class ConstructorTests
{
    private readonly Guid _categoryId = Guid.CreateVersion7();

    [Fact]
    public void KeepsTheCategoryIdAndTheValidationErrors()
    {
        List<string> validationErrors = ["First error.", "Second error."];

        CategoryValidationExceptionUnderTest exception = new(_categoryId, validationErrors);

        Assert.Equal(_categoryId, exception.CategoryId);
        Assert.Same(validationErrors, exception.ValidationErrors);
    }

    [Fact]
    public void NamesTheCategoryIdAndAllValidationErrorsInTheMessage()
    {
        CategoryValidationExceptionUnderTest exception = new(_categoryId, ["First error.", "Second error."]);

        Assert.Equal($"Category '{_categoryId}' is invalid: First error. Second error.", exception.Message);
    }
}
