using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Exceptions;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

namespace GwiOS.Core.Tests.ToDos.Infrastructure.ToDoPostgresRepository;

/// <summary>
/// Covers <c>ToDoPostgresRepository.UpdateAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class UpdateAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly PostgresTestDatabase _database = database;
    private readonly IToDoRepository _toDoRepository = database.ToDoRepository;
    private readonly TestToDos _testToDos = new(database.ToDoRepository);

    [Fact]
    public async Task OverwritesAllChangeablePropertiesOfTheStoredToDo()
    {
        ToDo toDo = _testToDos.Create();
        await _toDoRepository.InsertAsync(toDo, TestContext.Current.CancellationToken);
        ToDo changedToDo = new()
        {
            Id = toDo.Id,
            Title = _testToDos.CreateUniqueTitle(),
            Description = "Neue Beschreibung",
            IsCompleted = true,
            DueDate = new DateOnly(2026, 9, 30),
            CompletedAt = _testToDos.TruncateToMicroseconds(DateTime.UtcNow),
            Position = 7
        };

        await _toDoRepository.UpdateAsync(changedToDo, TestContext.Current.CancellationToken);

        ToDo? storedToDo = await _testToDos.FindStoredAsync(toDo.Id);
        Assert.NotNull(storedToDo);
        Assert.Equal(changedToDo.Title, storedToDo.Title);
        Assert.Equal(changedToDo.Description, storedToDo.Description);
        Assert.True(storedToDo.IsCompleted);
        Assert.Equal(changedToDo.DueDate, storedToDo.DueDate);
        Assert.Equal(changedToDo.CompletedAt, storedToDo.CompletedAt);
        Assert.Equal(changedToDo.Position, storedToDo.Position);
    }

    [Fact]
    public async Task KeepsTheCreationTimeOfTheStoredToDo()
    {
        ToDo toDo = _testToDos.Create();
        await _toDoRepository.InsertAsync(toDo, TestContext.Current.CancellationToken);
        ToDo changedToDo = new()
        {
            Id = toDo.Id,
            Title = toDo.Title,
            CreatedAt = toDo.CreatedAt.AddDays(-1)
        };

        await _toDoRepository.UpdateAsync(changedToDo, TestContext.Current.CancellationToken);

        Assert.Equal(toDo.CreatedAt, (await _testToDos.FindStoredAsync(toDo.Id))?.CreatedAt);
    }

    [Fact]
    public async Task ThrowsToDoNotFoundException_WhenTheToDoIsNotStored()
    {
        ToDo toDo = _testToDos.Create();

        ToDoNotFoundException exception = await Assert.ThrowsAsync<ToDoNotFoundException>(
            () => _toDoRepository.UpdateAsync(toDo, TestContext.Current.CancellationToken));

        Assert.Equal(toDo.Id, exception.ToDoId);
    }

    [Fact]
    public async Task LogsAWarningWithTheToDoId_WhenTheToDoIsNotStored()
    {
        ToDo toDo = _testToDos.Create();

        await Assert.ThrowsAsync<ToDoNotFoundException>(
            () => _toDoRepository.UpdateAsync(toDo, TestContext.Current.CancellationToken));

        Assert.Contains(
            _database.ToDoRepositoryLogger.Entries,
            logEntry => (logEntry.LogLevel == LogLevel.Warning)
                && (logEntry.ContextData.GetValueOrDefault("ToDoId") == toDo.Id.ToString()));
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testToDos.DisposeAsync();
}
