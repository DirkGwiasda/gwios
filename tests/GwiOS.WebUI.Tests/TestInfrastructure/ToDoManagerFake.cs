using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Exceptions;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

namespace GwiOS.WebUI.Tests.TestInfrastructure;

/// <summary>
/// Stands in for <see cref="IToDoManager"/> by keeping ToDos in memory, in insertion order. It completes ToDos at
/// <see cref="CompletionTime"/> and throws like the real manager when a ToDo to complete is not stored; it does not
/// validate.
/// </summary>
public sealed class ToDoManagerFake : IToDoManager
{
    private readonly List<ToDo> _toDos = [];

    /// <summary>
    /// The point in time completed ToDos are stamped with. Defaults to 2026-07-14 10:30 UTC.
    /// </summary>
    public DateTime CompletionTime { get; set; } = new(2026, 7, 14, 10, 30, 0, DateTimeKind.Utc);

    /// <summary>
    /// The stored ToDos in insertion order.
    /// </summary>
    public IReadOnlyList<ToDo> ToDos
        => _toDos;

    /// <summary>
    /// Stores the given ToDo directly, as if it had been created before the test.
    /// </summary>
    public void Add(ToDo toDo)
        => _toDos.Add(toDo);

    /// <summary>
    /// Deletes the ToDo with the given ID directly, as if someone else had deleted it.
    /// </summary>
    public void Remove(Guid toDoId)
        => _toDos.RemoveAll(toDo => toDo.Id == toDoId);

    public Task<ToDo> CreateToDoAsync(ToDo toDo, CancellationToken cancellationToken = default)
    {
        _toDos.Add(toDo);
        return Task.FromResult(toDo);
    }

    public Task CompleteToDoAsync(ToDo toDo, CancellationToken cancellationToken = default)
    {
        if (_toDos.All(storedToDo => storedToDo.Id != toDo.Id))
        {
            throw new ToDoNotFoundException(toDo.Id);
        }

        toDo.IsCompleted = true;
        toDo.CompletedAt = CompletionTime;
        return Task.CompletedTask;
    }

    public Task<List<ToDo>> GetAllToDosAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_toDos.ToList());

    public Task DeleteToDoAsync(Guid toDoId, CancellationToken cancellationToken = default)
    {
        Remove(toDoId);
        return Task.CompletedTask;
    }
}
