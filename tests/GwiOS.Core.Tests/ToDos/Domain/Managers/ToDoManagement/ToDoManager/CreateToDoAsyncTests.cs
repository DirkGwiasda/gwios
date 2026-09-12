using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Exceptions;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

namespace GwiOS.Core.Tests.ToDos.Domain.Managers.ToDoManagement.ToDoManager;

/// <summary>
/// Covers <c>ToDoManager.CreateToDoAsync</c>.
/// </summary>
public sealed class CreateToDoAsyncTests
{
    private readonly ToDoManagerProbe _probe = new();
    private readonly ToDo _toDo = new() { Title = "Steuerunterlagen sortieren" };

    [Fact]
    public async Task StoresTheToDo_WhenItIsValid()
    {
        await _probe.ToDoManager.CreateToDoAsync(_toDo, TestContext.Current.CancellationToken);

        List<ToDo> storedToDos = await _probe.ToDoRepository.GetAllAsync(TestContext.Current.CancellationToken);
        Assert.Same(_toDo, Assert.Single(storedToDos));
    }

    [Fact]
    public async Task ReturnsTheStoredToDo()
    {
        ToDo createdToDo = await _probe.ToDoManager.CreateToDoAsync(_toDo, TestContext.Current.CancellationToken);

        Assert.Same(_toDo, createdToDo);
    }

    [Fact]
    public async Task ValidatesTheToDo()
    {
        await _probe.ToDoManager.CreateToDoAsync(_toDo, TestContext.Current.CancellationToken);

        Assert.Same(_toDo, Assert.Single(_probe.ToDoValidator.ValidatedToDos));
    }

    [Fact]
    public async Task ThrowsToDoValidationExceptionWithTheValidationErrors_WhenTheToDoIsInvalid()
    {
        _probe.ToDoValidator.ValidationErrors = ["First error.", "Second error."];

        ToDoValidationException exception = await Assert.ThrowsAsync<ToDoValidationException>(
            () => _probe.ToDoManager.CreateToDoAsync(_toDo, TestContext.Current.CancellationToken));

        Assert.Equal(_toDo.Id, exception.ToDoId);
        Assert.Equal(["First error.", "Second error."], exception.ValidationErrors);
    }

    [Fact]
    public async Task DoesNotStoreTheToDo_WhenItIsInvalid()
    {
        _probe.ToDoValidator.ValidationErrors = ["Error."];

        await Assert.ThrowsAsync<ToDoValidationException>(
            () => _probe.ToDoManager.CreateToDoAsync(_toDo, TestContext.Current.CancellationToken));

        Assert.Empty(await _probe.ToDoRepository.GetAllAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LogsTheCreationWithTheToDoId()
    {
        await _probe.ToDoManager.CreateToDoAsync(_toDo, TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("ToDo created.", logEntry.Message);
        Assert.Equal(_toDo.Id.ToString(), logEntry.ContextData["ToDoId"]);
    }
}
