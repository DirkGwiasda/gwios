using GwiOS.Core.Tests.TestInfrastructure;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

namespace GwiOS.Core.Tests.ToDos.Infrastructure.ToDoPostgresRepository;

/// <summary>
/// Covers <c>ToDoPostgresRepository.DeleteAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class DeleteAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly IToDoRepository _toDoRepository = database.ToDoRepository;
    private readonly TestToDos _testToDos = new(database.ToDoRepository);

    [Fact]
    public async Task DeletesOnlyTheToDoWithTheGivenId()
    {
        ToDo toDoToDelete = _testToDos.Create();
        ToDo toDoToKeep = _testToDos.Create();
        await _toDoRepository.InsertAsync(toDoToDelete, TestContext.Current.CancellationToken);
        await _toDoRepository.InsertAsync(toDoToKeep, TestContext.Current.CancellationToken);

        await _toDoRepository.DeleteAsync(toDoToDelete.Id, TestContext.Current.CancellationToken);

        Assert.Null(await _testToDos.FindStoredAsync(toDoToDelete.Id));
        Assert.NotNull(await _testToDos.FindStoredAsync(toDoToKeep.Id));
    }

    [Fact]
    public async Task Succeeds_WhenTheToDoIsNotStored()
    {
        Exception? exception = await Record.ExceptionAsync(
            () => _toDoRepository.DeleteAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken));

        Assert.Null(exception);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testToDos.DisposeAsync();
}
