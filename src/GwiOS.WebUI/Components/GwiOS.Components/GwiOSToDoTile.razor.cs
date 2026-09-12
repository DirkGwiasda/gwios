using System.Globalization;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;
using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// Tile showing a single ToDo with buttons to complete and delete it. An open ToDo shows its due date, highlighted
/// once the date has passed; a completed ToDo is shown struck through and can only be deleted.
/// </summary>
public partial class GwiOSToDoTile
{
    /// <summary>
    /// The ToDo the tile shows.
    /// </summary>
    [Parameter, EditorRequired]
    public ToDo ToDo { get; set; } = default!;

    /// <summary>
    /// Raised when the user marks the open ToDo as completed.
    /// </summary>
    [Parameter]
    public EventCallback OnComplete { get; set; }

    /// <summary>
    /// Raised when the user deletes the ToDo.
    /// </summary>
    [Parameter]
    public EventCallback OnDelete { get; set; }

    [Inject]
    private TimeProvider TimeProvider { get; set; } = default!;

    private bool ShowsDueDate
        => !ToDo.IsCompleted && ToDo.DueDate is not null;

    private DateOnly Today
        => DateOnly.FromDateTime(TimeProvider.GetLocalNow().DateTime);

    private bool IsOverdue
        => ToDo.DueDate < Today;

    // The year is left out for dates in the current year, as in the design.
    private string DueDateLabel
        => ToDo.DueDate?.Year == Today.Year
            ? ToDo.DueDate.Value.ToString("dd.MM.", CultureInfo.InvariantCulture)
            : ToDo.DueDate?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) ?? string.Empty;
}
