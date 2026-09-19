namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Workflows.RecurringBookingGeneration.RecurringBookingGenerationWorkflow;

/// <summary>
/// Covers <c>RecurringBookingGenerationWorkflow.NeedsReviewAsync</c>.
/// </summary>
public sealed class NeedsReviewAsyncTests
{
    private readonly RecurringBookingGenerationWorkflowProbe _probe = new();

    [Fact]
    public async Task ReturnsTrue_WhenTheMonthWasNotReviewed()
    {
        bool needsReview = await _probe.Workflow.NeedsReviewAsync(
            new DateOnly(2026, 9, 15),
            TestContext.Current.CancellationToken);

        Assert.True(needsReview);
    }

    [Fact]
    public async Task ReturnsFalse_WhenTheMonthWasAlreadyReviewed()
    {
        _probe.MonthReviewManager.MarkAsAlreadyReviewed(new DateOnly(2026, 9, 1));

        bool needsReview = await _probe.Workflow.NeedsReviewAsync(
            new DateOnly(2026, 9, 15),
            TestContext.Current.CancellationToken);

        Assert.False(needsReview);
    }
}
