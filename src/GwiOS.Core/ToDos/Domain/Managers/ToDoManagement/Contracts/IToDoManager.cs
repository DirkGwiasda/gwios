using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Exceptions;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

namespace GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;

/// <summary>
/// Manages the ToDos of GwiOS: creates, completes, reads and deletes them, and rejects ToDos that are invalid.
/// </summary>
public interface IToDoManager
{
    /// <summary>
    /// Validates and stores the given new ToDo and returns it. Throws a <see cref="ToDoValidationException"/> if the
    /// ToDo is invalid.
    /// </summary>
    Task<ToDo> CreateToDoAsync(ToDo toDo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the given ToDo as completed at the current time and stores it. Throws a
    /// <see cref="ToDoNotFoundException"/> if no ToDo with its ID is stored.
    /// </summary>
    Task CompleteToDoAsync(ToDo toDo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all stored ToDos, ordered by position and then by creation time. The list is empty if no ToDos are
    /// stored.
    /// </summary>
    Task<List<ToDo>> GetAllToDosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the ToDo with the given ID. Does nothing if no such ToDo is stored.
    /// </summary>
    Task DeleteToDoAsync(Guid toDoId, CancellationToken cancellationToken = default);
}
