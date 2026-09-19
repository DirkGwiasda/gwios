using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts;

/// <summary>
/// Stands in for <see cref="IMonthReviewRepository"/> by keeping month reviews in memory. It upserts like the real
/// repository.
/// </summary>
public sealed class MonthReviewRepositoryFake : IMonthReviewRepository
{
    private readonly Dictionary<DateOnly, MonthReview> _monthReviews = [];

    public Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task UpsertAsync(MonthReview monthReview, CancellationToken cancellationToken = default)
    {
        _monthReviews[monthReview.Month] = monthReview;
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(DateOnly month, CancellationToken cancellationToken = default)
        => Task.FromResult(_monthReviews.ContainsKey(month));
}
