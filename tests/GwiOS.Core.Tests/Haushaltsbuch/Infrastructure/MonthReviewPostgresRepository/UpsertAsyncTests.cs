using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.MonthReviewPostgresRepository;

/// <summary>
/// Covers <c>MonthReviewPostgresRepository.UpsertAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class UpsertAsyncTests(PostgresTestDatabase database)
{
    private readonly IMonthReviewRepository _monthReviewRepository = database.MonthReviewRepository;

    [Fact]
    public async Task StoresANewMonthReview()
    {
        DateOnly month = TestMonths.CreateUnique();

        await _monthReviewRepository.UpsertAsync(
            new MonthReview { Month = month },
            TestContext.Current.CancellationToken);

        Assert.True(await _monthReviewRepository.ExistsAsync(month, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task OverwritesAnExistingMonthReview()
    {
        DateOnly month = TestMonths.CreateUnique();
        await _monthReviewRepository.UpsertAsync(
            new MonthReview { Month = month, ReviewedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            TestContext.Current.CancellationToken);

        await _monthReviewRepository.UpsertAsync(
            new MonthReview { Month = month, ReviewedAt = new DateTime(2026, 9, 18, 8, 0, 0, DateTimeKind.Utc) },
            TestContext.Current.CancellationToken);

        Assert.True(await _monthReviewRepository.ExistsAsync(month, TestContext.Current.CancellationToken));
    }
}
