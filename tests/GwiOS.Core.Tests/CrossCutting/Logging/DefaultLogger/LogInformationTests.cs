using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

namespace GwiOS.Core.Tests.CrossCutting.Logging.DefaultLogger;

/// <summary>
/// Covers the <c>DefaultLogger.LogInformation</c> overloads.
/// </summary>
public sealed class LogInformationTests : IDisposable
{
    private readonly DefaultLoggerProbe _probe = new();

    [Fact]
    public async Task WritesInformationEntryForTheDefaultApp()
    {
        _probe.Logger.LogInformation("Message");

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("GwiOS", logEntry.AppName);
        Assert.Equal(DefaultLoggerProbe.ExpectedDataSource, logEntry.DataSource);
        Assert.Equal("Message", logEntry.Message);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Empty(logEntry.ContextData);
    }

    [Fact]
    public async Task WritesInformationEntryForTheGivenApp()
    {
        _probe.Logger.LogInformation("Calendar", "Message");

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("Calendar", logEntry.AppName);
        Assert.Equal("Message", logEntry.Message);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
    }

    [Fact]
    public async Task WritesInformationEntryWithContextDataForTheDefaultApp()
    {
        _probe.Logger.LogInformation("Message", new Dictionary<string, string> { ["Key"] = "Value" });

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("GwiOS", logEntry.AppName);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Value", logEntry.ContextData["Key"]);
    }

    [Fact]
    public async Task WritesInformationEntryWithContextDataForTheGivenApp()
    {
        _probe.Logger.LogInformation("Calendar", "Message", new Dictionary<string, string> { ["Key"] = "Value" });

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("Calendar", logEntry.AppName);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Value", logEntry.ContextData["Key"]);
    }

    public void Dispose()
        => _probe.Dispose();
}
