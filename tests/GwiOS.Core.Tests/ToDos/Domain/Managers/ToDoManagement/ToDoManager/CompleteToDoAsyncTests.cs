using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Exceptions;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

namespace GwiOS.Core.Tests.ToDos.Domain.Managers.ToDoManagement.ToDoManager;

/// <summary>
/// Covers <c>ToDoManager.CompleteToDoAsync</c>.
/// </summary>
public sealed class CompleteToDoAsyncTests
{
    private readonly ToDoManagerProbe _probe = new();
    private readonly ToDo _toDo = new() { Title = "Zahnarzttermin vereinbaren" };

    [Fact]
    public async Task MarksTheToDoAsCompleted()
    {
        await _probe.ToDoRepository.InsertAsync(_toDo, TestContext.Current.CancellationToken);

        await _probe.ToDoManager.CompleteToDoAsync(_toDo, TestContext.Current.CancellationToken);

        Assert.True(_toDo.IsCompleted);
    }

    [Fact]
    public async Task SetsTheCompletionTimeToTheCurrentUtcTime()
    {
        await _probe.ToDoRepository.InsertAsync(_toDo, TestContext.Current.CancellationToken);

        await _probe.ToDoManager.CompleteToDoAsync(_toDo, TestContext.Current.CancellationToken);

        Assert.Equal(_probe.TimeProvider.UtcNow.UtcDateTime, _toDo.CompletedAt);
        Assert.Equal(DateTimeKind.Utc, _toDo.CompletedAt?.Kind);
    }

    [Fact]
    public async Task StoresTheCompletedToDo()
    {
        await _probe.ToDoRepository.InsertAsync(_toDo, TestContext.Current.CancellationToken);

        await _probe.ToDoManager.CompleteToDoAsync(_toDo, TestContext.Current.CancellationToken);

        ToDo updatedToDo = Assert.Single(_probe.ToDoRepository.UpdatedToDos);
        Assert.Same(_toDo, updatedToDo);
        Assert.True(updatedToDo.IsCompleted);
    }

    [Fact]
    public async Task ThrowsToDoNotFoundException_WhenTheToDoIsNotStored()
    {
        ToDoNotFoundException exception = await Assert.ThrowsAsync<ToDoNotFoundException>(
            () => _probe.ToDoManager.CompleteToDoAsync(_toDo, TestContext.Current.CancellationToken));

        Assert.Equal(_toDo.Id, exception.ToDoId);
    }

    [Fact]
    public async Task LogsTheCompletionWithTheToDoId()
    {
        await _probe.ToDoRepository.InsertAsync(_toDo, TestContext.Current.CancellationToken);

        await _probe.ToDoManager.CompleteToDoAsync(_toDo, TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("ToDo completed.", logEntry.Message);
        Assert.Equal(_toDo.Id.ToString(), logEntry.ContextData["ToDoId"]);
    }
}
