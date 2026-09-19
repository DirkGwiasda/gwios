using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Exceptions;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.MonthReviewManagement.MonthReviewManager;

/// <summary>
/// Covers <c>MonthReviewManager.MarkReviewedAsync</c>.
/// </summary>
public sealed class MarkReviewedAsyncTests
{
    private readonly MonthReviewManagerProbe _probe = new();
    private readonly DateOnly _month = new(2026, 9, 1);

    [Fact]
    public async Task MarksTheMonthAsReviewedAtTheCurrentTime()
    {
        _probe.TimeProvider.UtcNow = new DateTimeOffset(2026, 9, 18, 8, 0, 0, TimeSpan.Zero);

        await _probe.MonthReviewManager.MarkReviewedAsync(_month, TestContext.Current.CancellationToken);

        Assert.True(await _probe.MonthReviewRepository.ExistsAsync(_month, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task IsIdempotent_WhenTheMonthIsAlreadyReviewed()
    {
        await _probe.MonthReviewManager.MarkReviewedAsync(_month, TestContext.Current.CancellationToken);

        Exception? exception = await Record.ExceptionAsync(
            () => _probe.MonthReviewManager.MarkReviewedAsync(_month, TestContext.Current.CancellationToken));

        Assert.Null(exception);
    }

    [Fact]
    public async Task ThrowsMonthReviewValidationExceptionWithTheValidationErrors_WhenTheMonthReviewIsInvalid()
    {
        _probe.MonthReviewValidator.ValidationErrors = ["First error.", "Second error."];

        MonthReviewValidationException exception = await Assert.ThrowsAsync<MonthReviewValidationException>(
            () => _probe.MonthReviewManager.MarkReviewedAsync(_month, TestContext.Current.CancellationToken));

        Assert.Equal(_month, exception.Month);
        Assert.Equal(["First error.", "Second error."], exception.ValidationErrors);
    }

    [Fact]
    public async Task DoesNotStore_WhenTheMonthReviewIsInvalid()
    {
        _probe.MonthReviewValidator.ValidationErrors = ["Error."];

        await Assert.ThrowsAsync<MonthReviewValidationException>(
            () => _probe.MonthReviewManager.MarkReviewedAsync(_month, TestContext.Current.CancellationToken));

        Assert.False(await _probe.MonthReviewRepository.ExistsAsync(_month, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LogsTheReviewWithTheMonth()
    {
        await _probe.MonthReviewManager.MarkReviewedAsync(_month, TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Month marked as reviewed.", logEntry.Message);
        Assert.Equal(_month.ToString("O"), logEntry.ContextData["Month"]);
    }
}
