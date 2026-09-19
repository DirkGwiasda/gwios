using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using RecurringItemValidatorUnderTest = GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.RecurringItemValidator;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.RecurringItemValidator;

/// <summary>
/// Covers <c>RecurringItemValidator.IsValid</c>.
/// </summary>
public sealed class IsValidTests
{
    private readonly LoggerFake<RecurringItemValidatorUnderTest> _logger = new();
    private readonly RecurringItemValidatorUnderTest _recurringItemValidator;

    public IsValidTests()
    {
        _recurringItemValidator = new RecurringItemValidatorUnderTest(_logger);
    }

    [Fact]
    public void ReturnsTrueWithoutValidationErrors_WhenTheRecurringItemIsValid()
    {
        RecurringItem recurringItem = CreateValidRecurringItem();

        bool isValid = _recurringItemValidator.IsValid(recurringItem, out List<string> validationErrors);

        Assert.True(isValid);
        Assert.Empty(validationErrors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnsFalseWithAnError_WhenTheNameIsEmpty(string name)
    {
        RecurringItem recurringItem = CreateValidRecurringItem();
        recurringItem.Name = name;

        bool isValid = _recurringItemValidator.IsValid(recurringItem, out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Contains("The name must not be empty.", validationErrors);
    }

    [Fact]
    public void ReturnsFalseWithAnError_WhenTheCategoryIdIsEmpty()
    {
        RecurringItem recurringItem = CreateValidRecurringItem();
        recurringItem.CategoryId = Guid.Empty;

        bool isValid = _recurringItemValidator.IsValid(recurringItem, out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Contains("The category is required.", validationErrors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void ReturnsFalseWithAnError_WhenTheAmountIsNotGreaterThanZero(decimal amount)
    {
        RecurringItem recurringItem = CreateValidRecurringItem();
        recurringItem.Amount = amount;

        bool isValid = _recurringItemValidator.IsValid(recurringItem, out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Contains("The amount must be greater than 0.", validationErrors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(29)]
    public void ReturnsFalseWithAnError_WhenTheDueDayIsOutOfRange(int dueDay)
    {
        RecurringItem recurringItem = CreateValidRecurringItem();
        recurringItem.DueDay = dueDay;

        bool isValid = _recurringItemValidator.IsValid(recurringItem, out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Contains("The due day must be between 1 and 28.", validationErrors);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(12)]
    public void ReturnsFalseWithAnError_WhenTheStartMonthIsOutOfRange(int startMonth)
    {
        RecurringItem recurringItem = CreateValidRecurringItem();
        recurringItem.StartMonth = startMonth;

        bool isValid = _recurringItemValidator.IsValid(recurringItem, out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Contains("The start month must be between 0 and 11.", validationErrors);
    }

    [Fact]
    public void LogsAWarningWithTheRecurringItemIdAndTheErrors_WhenTheRecurringItemIsInvalid()
    {
        RecurringItem recurringItem = CreateValidRecurringItem();
        recurringItem.Name = "";

        _recurringItemValidator.IsValid(recurringItem, out _);

        LogEntry logEntry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Warning, logEntry.LogLevel);
        Assert.Equal(recurringItem.Id.ToString(), logEntry.ContextData["RecurringItemId"]);
        Assert.Contains("The name must not be empty.", logEntry.ContextData["ValidationErrors"]);
    }

    [Fact]
    public void LogsNothing_WhenTheRecurringItemIsValid()
    {
        RecurringItem recurringItem = CreateValidRecurringItem();

        _recurringItemValidator.IsValid(recurringItem, out _);

        Assert.Empty(_logger.Entries);
    }

    private static RecurringItem CreateValidRecurringItem()
        => new()
        {
            Name = "Miete",
            Type = CategoryType.Expense,
            CategoryId = Guid.CreateVersion7(),
            Amount = 1150m,
            Interval = RecurringInterval.Monthly
        };
}
