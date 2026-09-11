using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

namespace GwiOS.Core.Tests.CrossCutting.Logging.DefaultLogger;

/// <summary>
/// Covers the <c>DefaultLogger.LogWarning</c> overloads.
/// </summary>
public sealed class LogWarningTests : IDisposable
{
    private readonly DefaultLoggerProbe _probe = new();

    [Fact]
    public async Task WritesWarningEntryForTheDefaultApp()
    {
        _probe.Logger.LogWarning("Message");

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("GwiOS", logEntry.AppName);
        Assert.Equal(DefaultLoggerProbe.ExpectedDataSource, logEntry.DataSource);
        Assert.Equal("Message", logEntry.Message);
        Assert.Equal(LogLevel.Warning, logEntry.LogLevel);
        Assert.Empty(logEntry.ContextData);
    }

    [Fact]
    public async Task WritesWarningEntryForTheGivenApp()
    {
        _probe.Logger.LogWarning("Calendar", "Message");

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("Calendar", logEntry.AppName);
        Assert.Equal("Message", logEntry.Message);
        Assert.Equal(LogLevel.Warning, logEntry.LogLevel);
    }

    [Fact]
    public async Task WritesWarningEntryWithContextDataForTheDefaultApp()
    {
        _probe.Logger.LogWarning("Message", new Dictionary<string, string> { ["Key"] = "Value" });

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("GwiOS", logEntry.AppName);
        Assert.Equal(LogLevel.Warning, logEntry.LogLevel);
        Assert.Equal("Value", logEntry.ContextData["Key"]);
    }

    [Fact]
    public async Task WritesWarningEntryWithContextDataForTheGivenApp()
    {
        _probe.Logger.LogWarning("Calendar", "Message", new Dictionary<string, string> { ["Key"] = "Value" });

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("Calendar", logEntry.AppName);
        Assert.Equal(LogLevel.Warning, logEntry.LogLevel);
        Assert.Equal("Value", logEntry.ContextData["Key"]);
    }

    public void Dispose()
        => _probe.Dispose();
}
