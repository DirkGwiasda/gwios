using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Exceptions;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.Pages;

/// <summary>
/// Page listing the open and the completed ToDos, where ToDos are created, completed and deleted.
/// </summary>
public partial class ToDoOverview
{
    private List<ToDo> _toDos = [];
    private string _newTitle = string.Empty;
    private DateOnly? _newDueDate;

    [Inject]
    private IToDoManager ToDoManager { get; set; } = default!;

    [Inject]
    private IStatusMessageService StatusMessages { get; set; } = default!;

    private List<ToDo> OpenToDos
        => [.. _toDos.Where(toDo => !toDo.IsCompleted)];

    private List<ToDo> CompletedToDos
        => [.. _toDos.Where(toDo => toDo.IsCompleted)];

    private bool CanAddToDo
        => !string.IsNullOrWhiteSpace(_newTitle);

    protected override async Task OnInitializedAsync()
        => await LoadToDosAsync();

    private async Task AddToDoAsync()
    {
        if (!CanAddToDo)
        {
            return;
        }

        ToDo toDo = new() { Title = _newTitle.Trim(), DueDate = _newDueDate };
        await ToDoManager.CreateToDoAsync(toDo);
        _newTitle = string.Empty;
        _newDueDate = null;
        StatusMessages.Publish(StatusLevel.Neutral, $"ToDo „{toDo.Title}“ hinzugefügt");
        await LoadToDosAsync();
    }

    private async Task CompleteToDoAsync(ToDo toDo)
    {
        try
        {
            await ToDoManager.CompleteToDoAsync(toDo);
            StatusMessages.Publish(StatusLevel.Success, $"ToDo „{toDo.Title}“ erledigt");
        }
        catch (ToDoNotFoundException)
        {
            StatusMessages.Publish(StatusLevel.Warning, $"ToDo „{toDo.Title}“ wurde inzwischen gelöscht");
        }

        await LoadToDosAsync();
    }

    private async Task DeleteToDoAsync(ToDo toDo)
    {
        await ToDoManager.DeleteToDoAsync(toDo.Id);
        StatusMessages.Publish(StatusLevel.Neutral, $"ToDo „{toDo.Title}“ gelöscht");
        await LoadToDosAsync();
    }

    private async Task LoadToDosAsync()
        => _toDos = await ToDoManager.GetAllToDosAsync();
}
