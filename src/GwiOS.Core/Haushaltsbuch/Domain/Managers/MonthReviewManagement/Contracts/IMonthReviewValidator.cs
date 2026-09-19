using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts;

/// <summary>
/// Checks the values of a single month review.
/// </summary>
public interface IMonthReviewValidator
{
    /// <summary>
    /// Returns whether the given month review is valid. <paramref name="validationErrors"/> receives one message
    /// per violated rule; it is empty if the month review is valid.
    /// </summary>
    bool IsValid(MonthReview monthReview, out List<string> validationErrors);
}
