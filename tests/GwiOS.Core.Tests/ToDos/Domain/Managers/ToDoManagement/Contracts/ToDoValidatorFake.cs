using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

namespace GwiOS.Core.Tests.ToDos.Domain.Managers.ToDoManagement.Contracts;

/// <summary>
/// Stands in for <see cref="IToDoValidator"/>. It considers every ToDo valid until a test sets
/// <see cref="ValidationErrors"/>, and records every ToDo it validates.
/// </summary>
public sealed class ToDoValidatorFake : IToDoValidator
{
    private readonly List<ToDo> _validatedToDos = [];

    /// <summary>
    /// The validation errors reported for every ToDo. Empty by default, which makes every ToDo valid.
    /// </summary>
    public List<string> ValidationErrors { get; set; } = [];

    /// <summary>
    /// All ToDos validated so far, in the order of validation.
    /// </summary>
    public IReadOnlyList<ToDo> ValidatedToDos
        => _validatedToDos;

    public bool IsValid(ToDo toDo, out List<string> validationErrors)
    {
        _validatedToDos.Add(toDo);
        validationErrors = [.. ValidationErrors];
        return validationErrors.Count == 0;
    }
}
