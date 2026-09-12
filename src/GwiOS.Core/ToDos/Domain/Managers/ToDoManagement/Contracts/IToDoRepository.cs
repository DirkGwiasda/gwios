using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Exceptions;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

namespace GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;

/// <summary>
/// Persists, reads and deletes ToDos in the underlying storage.
/// </summary>
public interface IToDoRepository
{
    /// <summary>
    /// Creates the tables required for ToDos unless they already exist. The database itself must already exist.
    /// The operation is idempotent: calling it against existing tables changes nothing.
    /// </summary>
    Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores the given new ToDo.
    /// </summary>
    Task InsertAsync(ToDo toDo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Overwrites the stored ToDo with the ID of the given ToDo. Throws a <see cref="ToDoNotFoundException"/> if no
    /// such ToDo is stored.
    /// </summary>
    Task UpdateAsync(ToDo toDo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all stored ToDos, ordered by position and then by creation time. The list is empty if no ToDos are
    /// stored.
    /// </summary>
    Task<List<ToDo>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the ToDo with the given ID. Does nothing if no such ToDo is stored.
    /// </summary>
    Task DeleteAsync(Guid toDoId, CancellationToken cancellationToken = default);
}
