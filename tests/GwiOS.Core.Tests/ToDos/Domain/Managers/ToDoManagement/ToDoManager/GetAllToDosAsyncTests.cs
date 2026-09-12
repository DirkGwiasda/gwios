using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

namespace GwiOS.Core.Tests.ToDos.Domain.Managers.ToDoManagement.ToDoManager;

/// <summary>
/// Covers <c>ToDoManager.GetAllToDosAsync</c>.
/// </summary>
public sealed class GetAllToDosAsyncTests
{
    private readonly ToDoManagerProbe _probe = new();

    [Fact]
    public async Task ReturnsTheToDosOfTheRepositoryInItsOrder()
    {
        ToDo laterToDo = new() { Title = "Später", Position = 2 };
        ToDo earlierToDo = new() { Title = "Früher", Position = 1 };
        await _probe.ToDoRepository.InsertAsync(laterToDo, TestContext.Current.CancellationToken);
        await _probe.ToDoRepository.InsertAsync(earlierToDo, TestContext.Current.CancellationToken);

        List<ToDo> toDos = await _probe.ToDoManager.GetAllToDosAsync(TestContext.Current.CancellationToken);

        Assert.Equal([earlierToDo, laterToDo], toDos);
    }

    [Fact]
    public async Task ReturnsAnEmptyList_WhenNoToDosAreStored()
    {
        List<ToDo> toDos = await _probe.ToDoManager.GetAllToDosAsync(TestContext.Current.CancellationToken);

        Assert.Empty(toDos);
    }
}
