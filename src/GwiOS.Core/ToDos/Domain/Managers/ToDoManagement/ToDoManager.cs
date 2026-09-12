using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Exceptions;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;

namespace GwiOS.Core.ToDos.Domain.Managers.ToDoManagement;

internal sealed class ToDoManager(
    IToDoRepository toDoRepository,
    IToDoValidator toDoValidator,
    TimeProvider timeProvider,
    ILogger<ToDoManager> logger) : IToDoManager
{
    private readonly IToDoRepository _toDoRepository = toDoRepository;
    private readonly IToDoValidator _toDoValidator = toDoValidator;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<ToDoManager> _logger = logger;

    public async Task<ToDo> CreateToDoAsync(ToDo toDo, CancellationToken cancellationToken = default)
    {
        ThrowIfInvalid(toDo);
        await _toDoRepository.InsertAsync(toDo, cancellationToken);
        _logger.LogInformation("ToDo created.", CreateToDoIdData(toDo.Id));
        return toDo;
    }

    public async Task CompleteToDoAsync(ToDo toDo, CancellationToken cancellationToken = default)
    {
        toDo.IsCompleted = true;
        toDo.CompletedAt = _timeProvider.GetUtcNow().UtcDateTime;
        await _toDoRepository.UpdateAsync(toDo, cancellationToken);
        _logger.LogInformation("ToDo completed.", CreateToDoIdData(toDo.Id));
    }

    public async Task<List<ToDo>> GetAllToDosAsync(CancellationToken cancellationToken = default)
        => await _toDoRepository.GetAllAsync(cancellationToken);

    public async Task DeleteToDoAsync(Guid toDoId, CancellationToken cancellationToken = default)
    {
        await _toDoRepository.DeleteAsync(toDoId, cancellationToken);
        _logger.LogInformation("ToDo deleted.", CreateToDoIdData(toDoId));
    }

    private void ThrowIfInvalid(ToDo toDo)
    {
        if (!_toDoValidator.IsValid(toDo, out List<string> validationErrors))
        {
            throw new ToDoValidationException(toDo.Id, validationErrors);
        }
    }

    private static Dictionary<string, string> CreateToDoIdData(Guid toDoId)
        => new() { ["ToDoId"] = toDoId.ToString() };
}
