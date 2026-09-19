using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Models;
using Npgsql;
using NpgsqlTypes;

namespace GwiOS.Core.Haushaltsbuch.Infrastructure;

/// <summary>
/// Stores Haushaltsbuch month reviews in the table <c>haushaltsbuch.month_reviews</c> of the GwiOS PostgreSQL
/// database, using plain SQL.
/// </summary>
internal sealed class MonthReviewPostgresRepository(NpgsqlDataSource dataSource) : IMonthReviewRepository
{
    private const string CreateTablesSql = """
        CREATE SCHEMA IF NOT EXISTS haushaltsbuch;

        CREATE TABLE IF NOT EXISTS haushaltsbuch.month_reviews
        (
            month       date        NOT NULL,
            reviewed_at timestamptz NOT NULL,
            CONSTRAINT pk_haushaltsbuch_month_reviews PRIMARY KEY (month)
        );
        """;

    private const string UpsertSql = """
        INSERT INTO haushaltsbuch.month_reviews (month, reviewed_at)
        VALUES (@month, @reviewed_at)
        ON CONFLICT (month) DO UPDATE SET reviewed_at = EXCLUDED.reviewed_at
        """;

    private const string ExistsSql = """
        SELECT EXISTS (SELECT 1 FROM haushaltsbuch.month_reviews WHERE month = @month)
        """;

    private readonly NpgsqlDataSource _dataSource = dataSource;

    public async Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(CreateTablesSql);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpsertAsync(MonthReview monthReview, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(UpsertSql);
        command.Parameters.AddWithValue("month", NpgsqlDbType.Date, monthReview.Month);
        command.Parameters.AddWithValue("reviewed_at", NpgsqlDbType.TimestampTz, monthReview.ReviewedAt);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(DateOnly month, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(ExistsSql);
        command.Parameters.AddWithValue("month", NpgsqlDbType.Date, month);
        return (bool)(await command.ExecuteScalarAsync(cancellationToken))!;
    }
}
