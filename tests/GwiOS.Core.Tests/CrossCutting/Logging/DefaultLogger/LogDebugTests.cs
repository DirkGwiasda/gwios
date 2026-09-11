using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

namespace GwiOS.Core.Tests.CrossCutting.Logging.DefaultLogger;

/// <summary>
/// Covers the <c>DefaultLogger.LogDebug</c> overloads.
/// </summary>
public sealed class LogDebugTests : IDisposable
{
    private readonly DefaultLoggerProbe _probe = new();

    [Fact]
    public async Task WritesDebugEntryForTheDefaultApp()
    {
        _probe.Logger.LogDebug("Message");

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("GwiOS", logEntry.AppName);
        Assert.Equal(DefaultLoggerProbe.ExpectedDataSource, logEntry.DataSource);
        Assert.Equal("Message", logEntry.Message);
        Assert.Equal(LogLevel.Debug, logEntry.LogLevel);
        Assert.Empty(logEntry.ContextData);
    }

    [Fact]
    public async Task WritesDebugEntryForTheGivenApp()
    {
        _probe.Logger.LogDebug("Calendar", "Message");

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("Calendar", logEntry.AppName);
        Assert.Equal("Message", logEntry.Message);
        Assert.Equal(LogLevel.Debug, logEntry.LogLevel);
    }

    [Fact]
    public async Task WritesDebugEntryWithContextDataForTheDefaultApp()
    {
        _probe.Logger.LogDebug("Message", new Dictionary<string, string> { ["Key"] = "Value" });

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("GwiOS", logEntry.AppName);
        Assert.Equal(LogLevel.Debug, logEntry.LogLevel);
        Assert.Equal("Value", logEntry.ContextData["Key"]);
    }

    [Fact]
    public async Task WritesDebugEntryWithContextDataForTheGivenApp()
    {
        _probe.Logger.LogDebug("Calendar", "Message", new Dictionary<string, string> { ["Key"] = "Value" });

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("Calendar", logEntry.AppName);
        Assert.Equal(LogLevel.Debug, logEntry.LogLevel);
        Assert.Equal("Value", logEntry.ContextData["Key"]);
    }

    public void Dispose()
        => _probe.Dispose();
}
