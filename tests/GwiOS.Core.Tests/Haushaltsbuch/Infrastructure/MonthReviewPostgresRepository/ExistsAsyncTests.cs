using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.MonthReviewPostgresRepository;

/// <summary>
/// Covers <c>MonthReviewPostgresRepository.ExistsAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class ExistsAsyncTests(PostgresTestDatabase database)
{
    private readonly IMonthReviewRepository _monthReviewRepository = database.MonthReviewRepository;

    [Fact]
    public async Task ReturnsFalse_WhenTheMonthWasNeverReviewed()
    {
        DateOnly month = TestMonths.CreateUnique();

        bool exists = await _monthReviewRepository.ExistsAsync(month, TestContext.Current.CancellationToken);

        Assert.False(exists);
    }

    [Fact]
    public async Task ReturnsTrue_WhenTheMonthWasReviewed()
    {
        DateOnly month = TestMonths.CreateUnique();
        await _monthReviewRepository.UpsertAsync(
            new MonthReview { Month = month },
            TestContext.Current.CancellationToken);

        bool exists = await _monthReviewRepository.ExistsAsync(month, TestContext.Current.CancellationToken);

        Assert.True(exists);
    }
}
