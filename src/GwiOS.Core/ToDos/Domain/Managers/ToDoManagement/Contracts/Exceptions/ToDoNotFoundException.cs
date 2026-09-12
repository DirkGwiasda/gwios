namespace GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when an operation requires a stored ToDo, but no ToDo with the given ID is stored.
/// </summary>
/// <param name="toDoId">The ID that no stored ToDo has.</param>
public sealed class ToDoNotFoundException(Guid toDoId)
    : Exception($"No ToDo with ID '{toDoId}' is stored.")
{
    /// <summary>
    /// The ID that no stored ToDo has.
    /// </summary>
    public Guid ToDoId { get; } = toDoId;
}
