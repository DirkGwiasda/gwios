using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using LogEntryManagerUnderTest = GwiOS.Core.CrossCutting.Logging.Domain.Managers.LogEntryManager;

namespace GwiOS.Core.Tests.CrossCutting.Logging.Domain.Managers.LogEntryManager;

/// <summary>
/// Covers <c>LogEntryManager.GetLogEntryPageAsync</c>.
/// </summary>
public sealed class GetLogEntryPageAsyncTests
{
    private readonly LogEntryRepositoryFake _logEntryRepository = new();

    [Fact]
    public async Task ReturnsTheRequestedPageOfTheRepositoryAcrossAllApps()
    {
        DateTimeOffset timestamp = new(2026, 9, 12, 8, 0, 0, TimeSpan.Zero);
        LogEntry oldestEntry = new("Portal", "Tests", "Oldest", LogLevel.Information) { Timestamp = timestamp };
        LogEntry middleEntry = new("Calendar", "Tests", "Middle", LogLevel.Information)
        {
            Timestamp = timestamp.AddMinutes(1)
        };
        LogEntry newestEntry = new("Portal", "Tests", "Newest", LogLevel.Information)
        {
            Timestamp = timestamp.AddMinutes(2)
        };
        await _logEntryRepository.InsertAsync(oldestEntry);
        await _logEntryRepository.InsertAsync(middleEntry);
        await _logEntryRepository.InsertAsync(newestEntry);
        LogEntryManagerUnderTest logEntryManager = new(_logEntryRepository);

        LogEntryPage page = await logEntryManager.GetLogEntryPageAsync(2, 2);

        Assert.Same(oldestEntry, Assert.Single(page.Entries));
        Assert.Equal(3, page.TotalCount);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(-1, 10)]
    public async Task ThrowsAnArgumentOutOfRangeException_WhenPageNumberOrPageSizeIsLessThanOne(
        int pageNumber,
        int pageSize)
    {
        LogEntryManagerUnderTest logEntryManager = new(_logEntryRepository);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => logEntryManager.GetLogEntryPageAsync(pageNumber, pageSize));
    }
}
