using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using LogEntryUnderTest = GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models.LogEntry;

namespace GwiOS.Core.Tests.CrossCutting.Logging.Domain.Contracts.Models.LogEntry;

/// <summary>
/// Covers the constructor of <c>LogEntry</c> and the defaults it assigns.
/// </summary>
public sealed class ConstructorTests
{
    [Fact]
    public void SetsEmptyContextData_WhenNoContextDataIsGiven()
    {
        LogEntryUnderTest logEntry = new("Portal", "Tests", "Message", LogLevel.Information);

        Assert.Empty(logEntry.ContextData);
    }

    [Fact]
    public void KeepsTheGivenContextData()
    {
        Dictionary<string, string> contextData = new() { ["Key"] = "Value" };

        LogEntryUnderTest logEntry = new("Portal", "Tests", "Message", LogLevel.Information, contextData);

        Assert.Same(contextData, logEntry.ContextData);
    }

    [Fact]
    public void AssignsAVersion7Id()
    {
        LogEntryUnderTest logEntry = new("Portal", "Tests", "Message", LogLevel.Information);

        Assert.Equal(7, logEntry.Id.Version);
    }

    [Fact]
    public void AssignsADifferentIdToEachEntry()
    {
        LogEntryUnderTest firstLogEntry = new("Portal", "Tests", "Message", LogLevel.Information);
        LogEntryUnderTest secondLogEntry = new("Portal", "Tests", "Message", LogLevel.Information);

        Assert.NotEqual(firstLogEntry.Id, secondLogEntry.Id);
    }

    [Fact]
    public void SetsTheTimestampToTheCurrentUtcTime()
    {
        DateTimeOffset before = DateTimeOffset.UtcNow;

        LogEntryUnderTest logEntry = new("Portal", "Tests", "Message", LogLevel.Information);

        Assert.InRange(logEntry.Timestamp, before, DateTimeOffset.UtcNow);
        Assert.Equal(TimeSpan.Zero, logEntry.Timestamp.Offset);
    }
}
