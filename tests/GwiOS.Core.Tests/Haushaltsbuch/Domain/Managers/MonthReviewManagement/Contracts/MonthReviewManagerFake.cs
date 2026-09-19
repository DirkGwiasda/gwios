using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts;

/// <summary>
/// Stands in for <see cref="IMonthReviewManager"/> for the workflow tests. It keeps reviewed months in memory and
/// does not validate.
/// </summary>
public sealed class MonthReviewManagerFake : IMonthReviewManager
{
    private readonly HashSet<DateOnly> _reviewedMonths = [];

    /// <summary>
    /// The months marked as reviewed so far, in call order.
    /// </summary>
    public List<DateOnly> MarkedReviewedMonths { get; } = [];

    /// <summary>
    /// Marks the given month as already reviewed, as if it had been reviewed before the test.
    /// </summary>
    public void MarkAsAlreadyReviewed(DateOnly month)
        => _reviewedMonths.Add(month);

    public Task<bool> IsReviewedAsync(DateOnly month, CancellationToken cancellationToken = default)
        => Task.FromResult(_reviewedMonths.Contains(month));

    public Task MarkReviewedAsync(DateOnly month, CancellationToken cancellationToken = default)
    {
        _reviewedMonths.Add(month);
        MarkedReviewedMonths.Add(month);
        return Task.CompletedTask;
    }
}
