using MonthReviewValidationExceptionUnderTest =
    GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Exceptions.MonthReviewValidationException;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Exceptions.MonthReviewValidationException;

/// <summary>
/// Covers the constructor of <c>MonthReviewValidationException</c>.
/// </summary>
public sealed class ConstructorTests
{
    private readonly DateOnly _month = new(2026, 9, 1);

    [Fact]
    public void KeepsTheMonthAndTheValidationErrors()
    {
        List<string> validationErrors = ["First error.", "Second error."];

        MonthReviewValidationExceptionUnderTest exception = new(_month, validationErrors);

        Assert.Equal(_month, exception.Month);
        Assert.Same(validationErrors, exception.ValidationErrors);
    }

    [Fact]
    public void NamesTheMonthAndAllValidationErrorsInTheMessage()
    {
        MonthReviewValidationExceptionUnderTest exception = new(_month, ["First error.", "Second error."]);

        Assert.Equal($"Month review for '{_month}' is invalid: First error. Second error.", exception.Message);
    }
}
