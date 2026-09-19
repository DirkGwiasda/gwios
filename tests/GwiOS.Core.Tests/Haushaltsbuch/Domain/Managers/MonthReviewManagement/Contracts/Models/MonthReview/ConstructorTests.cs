using MonthReviewUnderTest = GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Models.MonthReview;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Models.MonthReview;

/// <summary>
/// Covers the construction of <c>MonthReview</c> and the defaults it assigns.
/// </summary>
public sealed class ConstructorTests
{
    [Fact]
    public void SetsTheReviewedAtTimeToTheCurrentUtcTime()
    {
        DateTime before = DateTime.UtcNow;

        MonthReviewUnderTest monthReview = new() { Month = new DateOnly(2026, 9, 1) };

        Assert.InRange(monthReview.ReviewedAt, before, DateTime.UtcNow);
        Assert.Equal(DateTimeKind.Utc, monthReview.ReviewedAt.Kind);
    }
}
