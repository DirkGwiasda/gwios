using ToDoValidationExceptionUnderTest =
    GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Exceptions.ToDoValidationException;

namespace GwiOS.Core.Tests.ToDos.Domain.Managers.ToDoManagement.Contracts.Exceptions.ToDoValidationException;

/// <summary>
/// Covers the constructor of <c>ToDoValidationException</c>.
/// </summary>
public sealed class ConstructorTests
{
    private readonly Guid _toDoId = Guid.CreateVersion7();

    [Fact]
    public void KeepsTheToDoIdAndTheValidationErrors()
    {
        List<string> validationErrors = ["First error.", "Second error."];

        ToDoValidationExceptionUnderTest exception = new(_toDoId, validationErrors);

        Assert.Equal(_toDoId, exception.ToDoId);
        Assert.Same(validationErrors, exception.ValidationErrors);
    }

    [Fact]
    public void NamesTheToDoIdAndAllValidationErrorsInTheMessage()
    {
        ToDoValidationExceptionUnderTest exception = new(_toDoId, ["First error.", "Second error."]);

        Assert.Equal($"ToDo '{_toDoId}' is invalid: First error. Second error.", exception.Message);
    }
}
