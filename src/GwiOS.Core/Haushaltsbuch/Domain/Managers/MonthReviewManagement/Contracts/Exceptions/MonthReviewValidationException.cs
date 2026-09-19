namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when a month review is rejected because it violates at least one validation rule.
/// </summary>
/// <param name="month">The month of the rejected review.</param>
/// <param name="validationErrors">One message per violated rule; never empty.</param>
public sealed class MonthReviewValidationException(DateOnly month, IReadOnlyList<string> validationErrors)
    : Exception($"Month review for '{month}' is invalid: {string.Join(" ", validationErrors)}")
{
    /// <summary>
    /// The month of the rejected review.
    /// </summary>
    public DateOnly Month { get; } = month;

    /// <summary>
    /// One message per violated rule; never empty.
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; } = validationErrors;
}
