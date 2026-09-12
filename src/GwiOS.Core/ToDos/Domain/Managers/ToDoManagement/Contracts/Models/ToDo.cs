namespace GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

/// <summary>
/// A task of the family that is either still open or already completed.
/// </summary>
public class ToDo
{
    /// <summary>
    /// The unique identifier of the ToDo. Defaults to a new time-ordered UUID (version 7).
    /// </summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>
    /// The title of the ToDo. Must not be empty.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// An optional longer description of the ToDo, or <c>null</c> if it has none.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the ToDo is completed. <c>false</c> while it is open.
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// The date the ToDo is due, or <c>null</c> if it has no due date.
    /// </summary>
    public DateOnly? DueDate { get; set; }

    /// <summary>
    /// Point in time the ToDo was created, in UTC. Defaults to the current UTC time at creation.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Point in time the ToDo was completed, in UTC, or <c>null</c> while it is open.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Sort order in the list of ToDos: ToDos are listed by ascending position, ToDos with the same position in the
    /// order of their creation.
    /// </summary>
    public int Position { get; set; }
}
