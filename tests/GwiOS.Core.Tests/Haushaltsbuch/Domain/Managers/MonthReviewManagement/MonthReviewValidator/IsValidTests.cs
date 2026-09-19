using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using MonthReviewValidatorUnderTest = GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.MonthReviewValidator;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.MonthReviewManagement.MonthReviewValidator;

/// <summary>
/// Covers <c>MonthReviewValidator.IsValid</c>.
/// </summary>
public sealed class IsValidTests
{
    private readonly LoggerFake<MonthReviewValidatorUnderTest> _logger = new();
    private readonly MonthReviewValidatorUnderTest _monthReviewValidator;

    public IsValidTests()
    {
        _monthReviewValidator = new MonthReviewValidatorUnderTest(_logger);
    }

    [Fact]
    public void ReturnsTrueWithoutValidationErrors_WhenTheMonthIsTheFirstOfItsMonth()
    {
        MonthReview monthReview = new() { Month = new DateOnly(2026, 9, 1) };

        bool isValid = _monthReviewValidator.IsValid(monthReview, out List<string> validationErrors);

        Assert.True(isValid);
        Assert.Empty(validationErrors);
    }

    [Fact]
    public void ReturnsFalseWithAnError_WhenTheMonthIsNotTheFirstOfItsMonth()
    {
        MonthReview monthReview = new() { Month = new DateOnly(2026, 9, 15) };

        bool isValid = _monthReviewValidator.IsValid(monthReview, out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Equal(["The month must be given as the 1st of its month."], validationErrors);
    }

    [Fact]
    public void LogsAWarningWithTheMonthAndTheErrors_WhenTheMonthReviewIsInvalid()
    {
        MonthReview monthReview = new() { Month = new DateOnly(2026, 9, 15) };

        _monthReviewValidator.IsValid(monthReview, out _);

        LogEntry logEntry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Warning, logEntry.LogLevel);
        Assert.Equal(monthReview.Month.ToString("O"), logEntry.ContextData["Month"]);
        Assert.Equal("The month must be given as the 1st of its month.", logEntry.ContextData["ValidationErrors"]);
    }

    [Fact]
    public void LogsNothing_WhenTheMonthReviewIsValid()
    {
        MonthReview monthReview = new() { Month = new DateOnly(2026, 9, 1) };

        _monthReviewValidator.IsValid(monthReview, out _);

        Assert.Empty(_logger.Entries);
    }
}
