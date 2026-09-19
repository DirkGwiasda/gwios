namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.MonthReviewPostgresRepository;

/// <summary>
/// Creates months for the tests against the PostgreSQL test database. Month reviews cannot be deleted, so tests use
/// a random month far outside any range a real Haushaltsbuch would use, to avoid colliding with other tests' data.
/// </summary>
public static class TestMonths
{
    /// <summary>
    /// Returns the 1st of a random month that no other test is likely to use.
    /// </summary>
    public static DateOnly CreateUnique()
        => new(Random.Shared.Next(1000, 8000), Random.Shared.Next(1, 13), 1);
}
