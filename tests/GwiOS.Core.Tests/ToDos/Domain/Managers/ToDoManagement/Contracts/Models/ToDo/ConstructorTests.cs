using ToDoUnderTest = GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models.ToDo;

namespace GwiOS.Core.Tests.ToDos.Domain.Managers.ToDoManagement.Contracts.Models.ToDo;

/// <summary>
/// Covers the construction of <c>ToDo</c> and the defaults it assigns.
/// </summary>
public sealed class ConstructorTests
{
    [Fact]
    public void AssignsAVersion7Id()
    {
        ToDoUnderTest toDo = new() { Title = "Einkaufen" };

        Assert.Equal(7, toDo.Id.Version);
    }

    [Fact]
    public void AssignsADifferentIdToEachToDo()
    {
        ToDoUnderTest firstToDo = new() { Title = "Einkaufen" };
        ToDoUnderTest secondToDo = new() { Title = "Einkaufen" };

        Assert.NotEqual(firstToDo.Id, secondToDo.Id);
    }

    [Fact]
    public void CreatesAnOpenToDoWithoutCompletionTime()
    {
        ToDoUnderTest toDo = new() { Title = "Einkaufen" };

        Assert.False(toDo.IsCompleted);
        Assert.Null(toDo.CompletedAt);
    }

    [Fact]
    public void SetsTheCreationTimeToTheCurrentUtcTime()
    {
        DateTime before = DateTime.UtcNow;

        ToDoUnderTest toDo = new() { Title = "Einkaufen" };

        Assert.InRange(toDo.CreatedAt, before, DateTime.UtcNow);
        Assert.Equal(DateTimeKind.Utc, toDo.CreatedAt.Kind);
    }

    [Fact]
    public void LeavesDescriptionAndDueDateUnset()
    {
        ToDoUnderTest toDo = new() { Title = "Einkaufen" };

        Assert.Null(toDo.Description);
        Assert.Null(toDo.DueDate);
    }
}
