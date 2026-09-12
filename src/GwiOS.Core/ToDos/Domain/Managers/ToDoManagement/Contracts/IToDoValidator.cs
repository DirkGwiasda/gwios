using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

namespace GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;

/// <summary>
/// Checks the values of a single ToDo.
/// </summary>
public interface IToDoValidator
{
    /// <summary>
    /// Returns whether the given ToDo is valid. <paramref name="validationErrors"/> receives one message per violated
    /// rule; it is empty if the ToDo is valid.
    /// </summary>
    bool IsValid(ToDo toDo, out List<string> validationErrors);
}
