using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using LogEntryManagerUnderTest = GwiOS.Core.CrossCutting.Logging.Domain.Managers.LogEntryManager;

namespace GwiOS.Core.Tests.CrossCutting.Logging.Domain.Managers.LogEntryManager;

/// <summary>
/// Covers <c>LogEntryManager.DeleteByAppNameAsync</c>.
/// </summary>
public sealed class DeleteByAppNameAsyncTests
{
    private readonly LogEntryRepositoryFake _logEntryRepository = new();

    [Fact]
    public async Task DeletesTheLogEntriesOfTheGivenAppFromTheRepository()
    {
        await _logEntryRepository.InsertAsync(new LogEntry("Portal", "Tests", "Message", LogLevel.Information));
        await _logEntryRepository.InsertAsync(new LogEntry("Calendar", "Tests", "Message", LogLevel.Information));
        LogEntryManagerUnderTest logEntryManager = new(_logEntryRepository);

        await logEntryManager.DeleteByAppNameAsync("Portal");

        string[] expectedAppNames = ["Calendar"];
        Assert.Equal(expectedAppNames, await _logEntryRepository.GetAllAppNamesAsync());
    }
}
