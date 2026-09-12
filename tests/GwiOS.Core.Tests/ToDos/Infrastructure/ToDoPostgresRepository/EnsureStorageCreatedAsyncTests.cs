using GwiOS.Core.Tests.TestInfrastructure;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

namespace GwiOS.Core.Tests.ToDos.Infrastructure.ToDoPostgresRepository;

/// <summary>
/// Covers <c>ToDoPostgresRepository.EnsureStorageCreatedAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class EnsureStorageCreatedAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly IToDoRepository _toDoRepository = database.ToDoRepository;
    private readonly TestToDos _testToDos = new(database.ToDoRepository);

    [Fact]
    public async Task KeepsExistingToDos_WhenTheTablesAlreadyExist()
    {
        ToDo toDo = _testToDos.Create();
        await _toDoRepository.InsertAsync(toDo, TestContext.Current.CancellationToken);

        await _toDoRepository.EnsureStorageCreatedAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(await _testToDos.FindStoredAsync(toDo.Id));
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testToDos.DisposeAsync();
}
