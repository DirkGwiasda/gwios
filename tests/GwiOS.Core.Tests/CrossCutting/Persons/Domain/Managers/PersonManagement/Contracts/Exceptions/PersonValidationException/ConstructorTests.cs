using PersonValidationExceptionUnderTest =
    GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions.PersonValidationException;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions.PersonValidationException;

/// <summary>
/// Covers the constructor of <c>PersonValidationException</c>.
/// </summary>
public sealed class ConstructorTests
{
    private readonly Guid _personId = Guid.CreateVersion7();

    [Fact]
    public void KeepsThePersonIdAndTheValidationErrors()
    {
        List<string> validationErrors = ["First error.", "Second error."];

        PersonValidationExceptionUnderTest exception = new(_personId, validationErrors);

        Assert.Equal(_personId, exception.PersonId);
        Assert.Same(validationErrors, exception.ValidationErrors);
    }

    [Fact]
    public void NamesThePersonIdAndAllValidationErrorsInTheMessage()
    {
        PersonValidationExceptionUnderTest exception = new(_personId, ["First error.", "Second error."]);

        Assert.Equal($"Person '{_personId}' is invalid: First error. Second error.", exception.Message);
    }
}
