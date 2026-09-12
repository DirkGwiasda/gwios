using GwiOS.Core.Tests.TestInfrastructure;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

namespace GwiOS.Core.Tests.ToDos.Infrastructure.ToDoPostgresRepository;

/// <summary>
/// Covers <c>ToDoPostgresRepository.InsertAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class InsertAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly IToDoRepository _toDoRepository = database.ToDoRepository;
    private readonly TestToDos _testToDos = new(database.ToDoRepository);

    [Fact]
    public async Task StoresAllPropertiesOfTheToDo()
    {
        ToDo toDo = _testToDos.Create();
        toDo.Description = "Belege für 2025";
        toDo.IsCompleted = true;
        toDo.DueDate = new DateOnly(2026, 8, 1);
        toDo.CompletedAt = _testToDos.TruncateToMicroseconds(DateTime.UtcNow);
        toDo.Position = 3;

        await _toDoRepository.InsertAsync(toDo, TestContext.Current.CancellationToken);

        ToDo? storedToDo = await _testToDos.FindStoredAsync(toDo.Id);
        Assert.NotNull(storedToDo);
        Assert.Equal(toDo.Title, storedToDo.Title);
        Assert.Equal(toDo.Description, storedToDo.Description);
        Assert.True(storedToDo.IsCompleted);
        Assert.Equal(toDo.DueDate, storedToDo.DueDate);
        Assert.Equal(toDo.CreatedAt, storedToDo.CreatedAt);
        Assert.Equal(toDo.CompletedAt, storedToDo.CompletedAt);
        Assert.Equal(toDo.Position, storedToDo.Position);
    }

    [Fact]
    public async Task StoresAToDoWithoutOptionalValues()
    {
        ToDo toDo = _testToDos.Create();

        await _toDoRepository.InsertAsync(toDo, TestContext.Current.CancellationToken);

        ToDo? storedToDo = await _testToDos.FindStoredAsync(toDo.Id);
        Assert.NotNull(storedToDo);
        Assert.Null(storedToDo.Description);
        Assert.Null(storedToDo.DueDate);
        Assert.Null(storedToDo.CompletedAt);
    }

    [Fact]
    public async Task ReadsTheTimesBackAsUtc()
    {
        ToDo toDo = _testToDos.Create();
        toDo.CompletedAt = _testToDos.TruncateToMicroseconds(DateTime.UtcNow);

        await _toDoRepository.InsertAsync(toDo, TestContext.Current.CancellationToken);

        ToDo? storedToDo = await _testToDos.FindStoredAsync(toDo.Id);
        Assert.NotNull(storedToDo);
        Assert.Equal(DateTimeKind.Utc, storedToDo.CreatedAt.Kind);
        Assert.Equal(DateTimeKind.Utc, storedToDo.CompletedAt?.Kind);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testToDos.DisposeAsync();
}
