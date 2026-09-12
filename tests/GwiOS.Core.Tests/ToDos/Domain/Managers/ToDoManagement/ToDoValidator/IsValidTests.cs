using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;
using ToDoValidatorUnderTest = GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.ToDoValidator;

namespace GwiOS.Core.Tests.ToDos.Domain.Managers.ToDoManagement.ToDoValidator;

/// <summary>
/// Covers <c>ToDoValidator.IsValid</c>.
/// </summary>
public sealed class IsValidTests
{
    private readonly LoggerFake<ToDoValidatorUnderTest> _logger = new();
    private readonly ToDoValidatorUnderTest _toDoValidator;

    public IsValidTests()
    {
        _toDoValidator = new ToDoValidatorUnderTest(_logger);
    }

    [Fact]
    public void ReturnsTrueWithoutValidationErrors_WhenTheTitleIsSet()
    {
        ToDo toDo = new() { Title = "Einkaufen" };

        bool isValid = _toDoValidator.IsValid(toDo, out List<string> validationErrors);

        Assert.True(isValid);
        Assert.Empty(validationErrors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnsFalseWithAnError_WhenTheTitleIsEmpty(string title)
    {
        ToDo toDo = new() { Title = title };

        bool isValid = _toDoValidator.IsValid(toDo, out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Equal(["The title must not be empty."], validationErrors);
    }

    [Fact]
    public void LogsAWarningWithTheToDoIdAndTheErrors_WhenTheToDoIsInvalid()
    {
        ToDo toDo = new() { Title = "" };

        _toDoValidator.IsValid(toDo, out _);

        LogEntry logEntry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Warning, logEntry.LogLevel);
        Assert.Equal(toDo.Id.ToString(), logEntry.ContextData["ToDoId"]);
        Assert.Equal("The title must not be empty.", logEntry.ContextData["ValidationErrors"]);
    }

    [Fact]
    public void LogsNothing_WhenTheToDoIsValid()
    {
        ToDo toDo = new() { Title = "Einkaufen" };

        _toDoValidator.IsValid(toDo, out _);

        Assert.Empty(_logger.Entries);
    }
}
