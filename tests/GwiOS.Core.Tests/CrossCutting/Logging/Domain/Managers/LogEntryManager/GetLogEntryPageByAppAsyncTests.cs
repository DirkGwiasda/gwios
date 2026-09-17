using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using LogEntryManagerUnderTest = GwiOS.Core.CrossCutting.Logging.Domain.Managers.LogEntryManager;

namespace GwiOS.Core.Tests.CrossCutting.Logging.Domain.Managers.LogEntryManager;

/// <summary>
/// Covers <c>LogEntryManager.GetLogEntryPageByAppAsync</c>.
/// </summary>
public sealed class GetLogEntryPageByAppAsyncTests
{
    private readonly LogEntryRepositoryFake _logEntryRepository = new();

    [Fact]
    public async Task ReturnsThePageOfTheRepositoryForTheGivenApp()
    {
        LogEntry portalEntry = new("Portal", "Tests", "Message", LogLevel.Information);
        await _logEntryRepository.InsertAsync(portalEntry);
        await _logEntryRepository.InsertAsync(new LogEntry("Calendar", "Tests", "Message", LogLevel.Information));
        LogEntryManagerUnderTest logEntryManager = new(_logEntryRepository);

        LogEntryPage page = await logEntryManager.GetLogEntryPageByAppAsync("Portal", 1, 10);

        Assert.Same(portalEntry, Assert.Single(page.Entries));
        Assert.Equal(1, page.TotalCount);
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
            () => logEntryManager.GetLogEntryPageByAppAsync("Portal", pageNumber, pageSize));
    }
}
