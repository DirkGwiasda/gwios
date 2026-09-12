using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Exceptions;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

namespace GwiOS.Core.Tests.ToDos.Domain.Managers.ToDoManagement.Contracts;

/// <summary>
/// Stands in for <see cref="IToDoRepository"/> by keeping ToDos in memory. It orders, updates and deletes like the
/// real repository and records every ToDo it is asked to update.
/// </summary>
public sealed class ToDoRepositoryFake : IToDoRepository
{
    private readonly List<ToDo> _toDos = [];
    private readonly List<ToDo> _updatedToDos = [];

    /// <summary>
    /// All ToDos passed to <see cref="UpdateAsync"/> so far, in call order, including those that were not stored.
    /// </summary>
    public IReadOnlyList<ToDo> UpdatedToDos
        => _updatedToDos;

    public Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task InsertAsync(ToDo toDo, CancellationToken cancellationToken = default)
    {
        _toDos.Add(toDo);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ToDo toDo, CancellationToken cancellationToken = default)
    {
        _updatedToDos.Add(toDo);
        int index = _toDos.FindIndex(storedToDo => storedToDo.Id == toDo.Id);
        if (index < 0)
        {
            throw new ToDoNotFoundException(toDo.Id);
        }

        _toDos[index] = toDo;
        return Task.CompletedTask;
    }

    public Task<List<ToDo>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_toDos.OrderBy(toDo => toDo.Position).ThenBy(toDo => toDo.CreatedAt).ToList());

    public Task DeleteAsync(Guid toDoId, CancellationToken cancellationToken = default)
    {
        _toDos.RemoveAll(toDo => toDo.Id == toDoId);
        return Task.CompletedTask;
    }
}
