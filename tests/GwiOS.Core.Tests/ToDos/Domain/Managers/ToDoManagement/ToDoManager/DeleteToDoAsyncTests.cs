using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

namespace GwiOS.Core.Tests.ToDos.Domain.Managers.ToDoManagement.ToDoManager;

/// <summary>
/// Covers <c>ToDoManager.DeleteToDoAsync</c>.
/// </summary>
public sealed class DeleteToDoAsyncTests
{
    private readonly ToDoManagerProbe _probe = new();
    private readonly ToDo _shopping = new() { Title = "Einkaufen" };
    private readonly ToDo _taxes = new() { Title = "Steuern" };

    [Fact]
    public async Task DeletesOnlyTheToDoWithTheGivenId()
    {
        await _probe.ToDoRepository.InsertAsync(_shopping, TestContext.Current.CancellationToken);
        await _probe.ToDoRepository.InsertAsync(_taxes, TestContext.Current.CancellationToken);

        await _probe.ToDoManager.DeleteToDoAsync(_shopping.Id, TestContext.Current.CancellationToken);

        List<ToDo> storedToDos = await _probe.ToDoRepository.GetAllAsync(TestContext.Current.CancellationToken);
        Assert.Same(_taxes, Assert.Single(storedToDos));
    }

    [Fact]
    public async Task Succeeds_WhenTheToDoIsNotStored()
    {
        Exception? exception = await Record.ExceptionAsync(
            () => _probe.ToDoManager.DeleteToDoAsync(_shopping.Id, TestContext.Current.CancellationToken));

        Assert.Null(exception);
    }

    [Fact]
    public async Task LogsTheDeletionWithTheToDoId()
    {
        await _probe.ToDoRepository.InsertAsync(_shopping, TestContext.Current.CancellationToken);

        await _probe.ToDoManager.DeleteToDoAsync(_shopping.Id, TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("ToDo deleted.", logEntry.Message);
        Assert.Equal(_shopping.Id.ToString(), logEntry.ContextData["ToDoId"]);
    }
}
