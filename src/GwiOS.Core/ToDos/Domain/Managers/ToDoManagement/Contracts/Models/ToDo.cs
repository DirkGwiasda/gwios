namespace GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

public class ToDo
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    /// <summary>
    /// Sort order in view list
    /// </summary>
    public int Position { get; set; }
}
