using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts;

/// <summary>
/// Stands in for <see cref="IMonthReviewValidator"/>. It considers every month review valid until a test sets
/// <see cref="ValidationErrors"/>.
/// </summary>
public sealed class MonthReviewValidatorFake : IMonthReviewValidator
{
    /// <summary>
    /// The validation errors reported for every month review. Empty by default, which makes every month review
    /// valid.
    /// </summary>
    public List<string> ValidationErrors { get; set; } = [];

    public bool IsValid(MonthReview monthReview, out List<string> validationErrors)
    {
        validationErrors = [.. ValidationErrors];
        return validationErrors.Count == 0;
    }
}
