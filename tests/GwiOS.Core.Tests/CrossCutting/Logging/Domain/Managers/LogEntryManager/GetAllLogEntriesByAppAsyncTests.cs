using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using LogEntryManagerUnderTest = GwiOS.Core.CrossCutting.Logging.Domain.Managers.LogEntryManager;

namespace GwiOS.Core.Tests.CrossCutting.Logging.Domain.Managers.LogEntryManager;

/// <summary>
/// Covers <c>LogEntryManager.GetAllLogEntriesByAppAsync</c>.
/// </summary>
public sealed class GetAllLogEntriesByAppAsyncTests
{
    private readonly LogEntryRepositoryFake _logEntryRepository = new();

    [Fact]
    public async Task ReturnsTheLogEntriesOfTheRepositoryForTheGivenApp()
    {
        LogEntry portalEntry = new("Portal", "Tests", "Message", LogLevel.Information);
        await _logEntryRepository.InsertAsync(portalEntry);
        await _logEntryRepository.InsertAsync(new LogEntry("Calendar", "Tests", "Message", LogLevel.Information));
        LogEntryManagerUnderTest logEntryManager = new(_logEntryRepository);

        List<LogEntry> logEntries = await logEntryManager.GetAllLogEntriesByAppAsync("Portal");

        Assert.Same(portalEntry, Assert.Single(logEntries));
    }
}
