using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using LogEntryManagerUnderTest = GwiOS.Core.CrossCutting.Logging.Domain.Managers.LogEntryManager;

namespace GwiOS.Core.Tests.CrossCutting.Logging.Domain.Managers.LogEntryManager;

/// <summary>
/// Covers <c>LogEntryManager.GetAllAppNamesAsync</c>.
/// </summary>
public sealed class GetAllAppNamesAsyncTests
{
    private readonly LogEntryRepositoryFake _logEntryRepository = new();

    [Fact]
    public async Task ReturnsTheAppNamesOfTheRepository()
    {
        await _logEntryRepository.InsertAsync(new LogEntry("Portal", "Tests", "Message", LogLevel.Information));
        await _logEntryRepository.InsertAsync(new LogEntry("Calendar", "Tests", "Message", LogLevel.Information));
        LogEntryManagerUnderTest logEntryManager = new(_logEntryRepository);

        List<string> appNames = await logEntryManager.GetAllAppNamesAsync();

        string[] expectedAppNames = ["Calendar", "Portal"];
        Assert.Equal(expectedAppNames, appNames);
    }
}
