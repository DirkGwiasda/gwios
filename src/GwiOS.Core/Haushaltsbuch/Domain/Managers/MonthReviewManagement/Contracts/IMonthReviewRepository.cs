using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts;

/// <summary>
/// Persists and reads month reviews in the underlying storage.
/// </summary>
public interface IMonthReviewRepository
{
    /// <summary>
    /// Creates the tables required for month reviews unless they already exist. The database itself must already
    /// exist. The operation is idempotent: calling it against existing tables changes nothing.
    /// </summary>
    Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores the given month review, overwriting any existing review of the same month.
    /// </summary>
    Task UpsertAsync(MonthReview monthReview, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns whether the given month has been reviewed.
    /// </summary>
    Task<bool> ExistsAsync(DateOnly month, CancellationToken cancellationToken = default);
}
