using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

namespace GwiOS.Core.ToDos.Domain.Managers.ToDoManagement;

internal sealed class ToDoValidator(ILogger<ToDoValidator> logger) : IToDoValidator
{
    private readonly ILogger<ToDoValidator> _logger = logger;

    public bool IsValid(ToDo toDo, out List<string> validationErrors)
    {
        validationErrors = CollectValidationErrors(toDo);
        if (validationErrors.Count == 0)
        {
            return true;
        }

        LogValidationErrors(toDo, validationErrors);
        return false;
    }

    private static List<string> CollectValidationErrors(ToDo toDo)
    {
        List<string> validationErrors = [];
        if (string.IsNullOrWhiteSpace(toDo.Title))
        {
            validationErrors.Add("The title must not be empty.");
        }

        return validationErrors;
    }

    private void LogValidationErrors(ToDo toDo, List<string> validationErrors)
        => _logger.LogWarning(
            "ToDo is invalid.",
            new Dictionary<string, string>
            {
                ["ToDoId"] = toDo.Id.ToString(),
                ["ValidationErrors"] = string.Join(" ", validationErrors)
            });
}
