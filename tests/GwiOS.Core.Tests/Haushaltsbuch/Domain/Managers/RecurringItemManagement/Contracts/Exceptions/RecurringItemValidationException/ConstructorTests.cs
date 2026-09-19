using RecurringItemValidationExceptionUnderTest =
    GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Exceptions.RecurringItemValidationException;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Exceptions.RecurringItemValidationException;

/// <summary>
/// Covers the constructor of <c>RecurringItemValidationException</c>.
/// </summary>
public sealed class ConstructorTests
{
    private readonly Guid _recurringItemId = Guid.CreateVersion7();

    [Fact]
    public void KeepsTheRecurringItemIdAndTheValidationErrors()
    {
        List<string> validationErrors = ["First error.", "Second error."];

        RecurringItemValidationExceptionUnderTest exception = new(_recurringItemId, validationErrors);

        Assert.Equal(_recurringItemId, exception.RecurringItemId);
        Assert.Same(validationErrors, exception.ValidationErrors);
    }

    [Fact]
    public void NamesTheRecurringItemIdAndAllValidationErrorsInTheMessage()
    {
        RecurringItemValidationExceptionUnderTest exception = new(_recurringItemId, ["First error.", "Second error."]);

        Assert.Equal($"Recurring item '{_recurringItemId}' is invalid: First error. Second error.", exception.Message);
    }
}
