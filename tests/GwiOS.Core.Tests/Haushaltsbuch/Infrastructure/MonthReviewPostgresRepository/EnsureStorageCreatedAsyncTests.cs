using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.MonthReviewPostgresRepository;

/// <summary>
/// Covers <c>MonthReviewPostgresRepository.EnsureStorageCreatedAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class EnsureStorageCreatedAsyncTests(PostgresTestDatabase database)
{
    private readonly IMonthReviewRepository _monthReviewRepository = database.MonthReviewRepository;

    [Fact]
    public async Task KeepsExistingMonthReviews_WhenTheTablesAlreadyExist()
    {
        DateOnly month = TestMonths.CreateUnique();
        await _monthReviewRepository.UpsertAsync(
            new MonthReview { Month = month },
            TestContext.Current.CancellationToken);

        await _monthReviewRepository.EnsureStorageCreatedAsync(TestContext.Current.CancellationToken);

        Assert.True(await _monthReviewRepository.ExistsAsync(month, TestContext.Current.CancellationToken));
    }
}
