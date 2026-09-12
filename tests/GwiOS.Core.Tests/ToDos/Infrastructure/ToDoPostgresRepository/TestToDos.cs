using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

namespace GwiOS.Core.Tests.ToDos.Infrastructure.ToDoPostgresRepository;

/// <summary>
/// Creates ToDos for the tests against the PostgreSQL test database and deletes every ToDo it created from the
/// database when it is disposed. The database may contain ToDos of other tests, so tests only look at the ToDos they
/// created themselves.
/// </summary>
public sealed class TestToDos(IToDoRepository toDoRepository) : IAsyncDisposable
{
    private readonly IToDoRepository _toDoRepository = toDoRepository;
    private readonly List<Guid> _toDoIds = [];

    /// <summary>
    /// Returns a new, not yet stored, open ToDo with a unique title and without optional values. Its creation time
    /// is cut to whole microseconds, the precision PostgreSQL stores, so it survives a round trip unchanged.
    /// </summary>
    public ToDo Create()
        => Create(TruncateToMicroseconds(DateTime.UtcNow));

    /// <summary>
    /// Returns a new, not yet stored, open ToDo with a unique title, the given creation time and without optional
    /// values.
    /// </summary>
    public ToDo Create(DateTime createdAt)
    {
        ToDo toDo = new() { Title = CreateUniqueTitle(), CreatedAt = createdAt };
        _toDoIds.Add(toDo.Id);
        return toDo;
    }

    /// <summary>
    /// Returns a title that no other ToDo uses, prefixed so that leftovers are recognizable as test data.
    /// </summary>
    public string CreateUniqueTitle()
        => $"test-{Guid.NewGuid():N}";

    /// <summary>
    /// Returns the given UTC time cut to whole microseconds, the precision PostgreSQL stores.
    /// </summary>
    public DateTime TruncateToMicroseconds(DateTime utcTime)
        => new(utcTime.Ticks - (utcTime.Ticks % TimeSpan.TicksPerMicrosecond), DateTimeKind.Utc);

    /// <summary>
    /// Returns the stored ToDo with the given ID, or <c>null</c> if no such ToDo is stored.
    /// </summary>
    public async Task<ToDo?> FindStoredAsync(Guid toDoId)
        => (await _toDoRepository.GetAllAsync()).SingleOrDefault(toDo => toDo.Id == toDoId);

    public async ValueTask DisposeAsync()
    {
        foreach (Guid toDoId in _toDoIds)
        {
            await _toDoRepository.DeleteAsync(toDoId);
        }
    }
}
