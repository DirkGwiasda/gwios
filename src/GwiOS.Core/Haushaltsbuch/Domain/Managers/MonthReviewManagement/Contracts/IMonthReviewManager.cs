using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Exceptions;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts;

/// <summary>
/// Manages which months of the Haushaltsbuch have been reviewed: marks a month as reviewed and reports whether a
/// given month has been reviewed yet.
/// </summary>
public interface IMonthReviewManager
{
    /// <summary>
    /// Returns whether the given month has already been reviewed.
    /// </summary>
    Task<bool> IsReviewedAsync(DateOnly month, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the given month as reviewed, at the current time. Idempotent: reviewing an already reviewed month just
    /// updates the time. Throws a <see cref="MonthReviewValidationException"/> if <paramref name="month"/> is not
    /// the 1st of its month.
    /// </summary>
    Task MarkReviewedAsync(DateOnly month, CancellationToken cancellationToken = default);
}
