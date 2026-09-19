namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.MonthReviewManagement.MonthReviewManager;

/// <summary>
/// Covers <c>MonthReviewManager.IsReviewedAsync</c>.
/// </summary>
public sealed class IsReviewedAsyncTests
{
    private readonly MonthReviewManagerProbe _probe = new();
    private readonly DateOnly _month = new(2026, 9, 1);

    [Fact]
    public async Task ReturnsFalse_WhenTheMonthWasNotReviewed()
    {
        bool isReviewed = await _probe.MonthReviewManager.IsReviewedAsync(_month, TestContext.Current.CancellationToken);

        Assert.False(isReviewed);
    }

    [Fact]
    public async Task ReturnsTrue_WhenTheMonthWasReviewed()
    {
        await _probe.MonthReviewManager.MarkReviewedAsync(_month, TestContext.Current.CancellationToken);

        bool isReviewed = await _probe.MonthReviewManager.IsReviewedAsync(_month, TestContext.Current.CancellationToken);

        Assert.True(isReviewed);
    }
}
