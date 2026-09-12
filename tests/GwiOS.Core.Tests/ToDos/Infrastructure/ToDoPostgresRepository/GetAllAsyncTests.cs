using GwiOS.Core.Tests.TestInfrastructure;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

namespace GwiOS.Core.Tests.ToDos.Infrastructure.ToDoPostgresRepository;

/// <summary>
/// Covers <c>ToDoPostgresRepository.GetAllAsync</c> against the PostgreSQL test database. The database may contain
/// ToDos of other tests, so the tests only look at the ToDos they created themselves.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class GetAllAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly IToDoRepository _toDoRepository = database.ToDoRepository;
    private readonly TestToDos _testToDos = new(database.ToDoRepository);

    [Fact]
    public async Task ReturnsEveryStoredToDoOnce()
    {
        ToDo firstToDo = _testToDos.Create();
        ToDo secondToDo = _testToDos.Create();
        await _toDoRepository.InsertAsync(firstToDo, TestContext.Current.CancellationToken);
        await _toDoRepository.InsertAsync(secondToDo, TestContext.Current.CancellationToken);

        List<ToDo> toDos = await _toDoRepository.GetAllAsync(TestContext.Current.CancellationToken);

        Assert.Single(toDos, toDo => toDo.Id == firstToDo.Id);
        Assert.Single(toDos, toDo => toDo.Id == secondToDo.Id);
    }

    [Fact]
    public async Task ReturnsTheToDosOrderedByPosition()
    {
        ToDo secondToDo = _testToDos.Create();
        secondToDo.Position = 2;
        ToDo firstToDo = _testToDos.Create();
        firstToDo.Position = 1;
        await _toDoRepository.InsertAsync(secondToDo, TestContext.Current.CancellationToken);
        await _toDoRepository.InsertAsync(firstToDo, TestContext.Current.CancellationToken);

        List<ToDo> toDos = await _toDoRepository.GetAllAsync(TestContext.Current.CancellationToken);

        AssertOrder([firstToDo.Id, secondToDo.Id], toDos);
    }

    [Fact]
    public async Task ReturnsToDosWithTheSamePositionOrderedByCreationTime()
    {
        DateTime now = _testToDos.TruncateToMicroseconds(DateTime.UtcNow);
        ToDo newerToDo = _testToDos.Create(now);
        ToDo olderToDo = _testToDos.Create(now.AddMinutes(-1));
        await _toDoRepository.InsertAsync(newerToDo, TestContext.Current.CancellationToken);
        await _toDoRepository.InsertAsync(olderToDo, TestContext.Current.CancellationToken);

        List<ToDo> toDos = await _toDoRepository.GetAllAsync(TestContext.Current.CancellationToken);

        AssertOrder([olderToDo.Id, newerToDo.Id], toDos);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testToDos.DisposeAsync();

    private static void AssertOrder(Guid[] expectedIds, List<ToDo> toDos)
        => Assert.Equal(expectedIds, toDos.Where(toDo => expectedIds.Contains(toDo.Id)).Select(toDo => toDo.Id));
}
