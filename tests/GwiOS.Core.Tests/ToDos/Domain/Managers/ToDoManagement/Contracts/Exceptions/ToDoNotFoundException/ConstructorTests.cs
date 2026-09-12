using ToDoNotFoundExceptionUnderTest =
    GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Exceptions.ToDoNotFoundException;

namespace GwiOS.Core.Tests.ToDos.Domain.Managers.ToDoManagement.Contracts.Exceptions.ToDoNotFoundException;

/// <summary>
/// Covers the constructor of <c>ToDoNotFoundException</c>.
/// </summary>
public sealed class ConstructorTests
{
    private readonly Guid _toDoId = Guid.CreateVersion7();

    [Fact]
    public void KeepsTheToDoId()
    {
        ToDoNotFoundExceptionUnderTest exception = new(_toDoId);

        Assert.Equal(_toDoId, exception.ToDoId);
    }

    [Fact]
    public void NamesTheToDoIdInTheMessage()
    {
        ToDoNotFoundExceptionUnderTest exception = new(_toDoId);

        Assert.Equal($"No ToDo with ID '{_toDoId}' is stored.", exception.Message);
    }
}
