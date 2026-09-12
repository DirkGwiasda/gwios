namespace GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when a ToDo is rejected because it violates at least one validation rule.
/// </summary>
/// <param name="toDoId">The ID of the rejected ToDo.</param>
/// <param name="validationErrors">One message per violated rule; never empty.</param>
public sealed class ToDoValidationException(Guid toDoId, IReadOnlyList<string> validationErrors)
    : Exception($"ToDo '{toDoId}' is invalid: {string.Join(" ", validationErrors)}")
{
    /// <summary>
    /// The ID of the rejected ToDo.
    /// </summary>
    public Guid ToDoId { get; } = toDoId;

    /// <summary>
    /// One message per violated rule; never empty.
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; } = validationErrors;
}
