using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

namespace GwiOS.Core.Tests.CrossCutting.Logging.DefaultLogger;

/// <summary>
/// Covers the <c>DefaultLogger.LogError</c> overloads, including those that describe an exception.
/// </summary>
public sealed class LogErrorTests : IDisposable
{
    private readonly DefaultLoggerProbe _probe = new();

    [Fact]
    public async Task WritesErrorEntryForTheDefaultApp()
    {
        _probe.Logger.LogError("Message");

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("GwiOS", logEntry.AppName);
        Assert.Equal(DefaultLoggerProbe.ExpectedDataSource, logEntry.DataSource);
        Assert.Equal("Message", logEntry.Message);
        Assert.Equal(LogLevel.Error, logEntry.LogLevel);
        Assert.Empty(logEntry.ContextData);
    }

    [Fact]
    public async Task WritesErrorEntryForTheGivenApp()
    {
        _probe.Logger.LogError("Calendar", "Message");

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("Calendar", logEntry.AppName);
        Assert.Equal("Message", logEntry.Message);
        Assert.Equal(LogLevel.Error, logEntry.LogLevel);
    }

    [Fact]
    public async Task WritesErrorEntryWithContextDataForTheDefaultApp()
    {
        _probe.Logger.LogError("Message", new Dictionary<string, string> { ["Key"] = "Value" });

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("GwiOS", logEntry.AppName);
        Assert.Equal(LogLevel.Error, logEntry.LogLevel);
        Assert.Equal("Value", logEntry.ContextData["Key"]);
    }

    [Fact]
    public async Task WritesErrorEntryWithContextDataForTheGivenApp()
    {
        _probe.Logger.LogError("Calendar", "Message", new Dictionary<string, string> { ["Key"] = "Value" });

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("Calendar", logEntry.AppName);
        Assert.Equal(LogLevel.Error, logEntry.LogLevel);
        Assert.Equal("Value", logEntry.ContextData["Key"]);
    }

    [Fact]
    public async Task WritesErrorEntryDescribingTheExceptionForTheDefaultApp()
    {
        _probe.Logger.LogError(CreateThrownException());

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("GwiOS", logEntry.AppName);
        Assert.Equal(LogLevel.Error, logEntry.LogLevel);
        AssertDescribesThrownException(logEntry);
    }

    [Fact]
    public async Task WritesErrorEntryDescribingTheExceptionForTheGivenApp()
    {
        _probe.Logger.LogError("Calendar", CreateThrownException());

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("Calendar", logEntry.AppName);
        Assert.Equal(LogLevel.Error, logEntry.LogLevel);
        AssertDescribesThrownException(logEntry);
    }

    [Fact]
    public async Task WritesErrorEntryDescribingTheExceptionWithContextDataForTheDefaultApp()
    {
        _probe.Logger.LogError(CreateThrownException(), new Dictionary<string, string> { ["Key"] = "Value" });

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("GwiOS", logEntry.AppName);
        Assert.Equal(LogLevel.Error, logEntry.LogLevel);
        AssertDescribesThrownException(logEntry);
        Assert.Equal("Value", logEntry.ContextData["Key"]);
    }

    [Fact]
    public async Task WritesErrorEntryDescribingTheExceptionWithContextDataForTheGivenApp()
    {
        _probe.Logger.LogError("Calendar", CreateThrownException(), new Dictionary<string, string> { ["Key"] = "Value" });

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal("Calendar", logEntry.AppName);
        Assert.Equal(LogLevel.Error, logEntry.LogLevel);
        AssertDescribesThrownException(logEntry);
        Assert.Equal("Value", logEntry.ContextData["Key"]);
    }

    [Fact]
    public async Task KeepsTheExceptionDetails_WhenTheContextDataUsesTheSameKey()
    {
        _probe.Logger.LogError(CreateThrownException(), new Dictionary<string, string> { ["ExceptionType"] = "Other" });

        LogEntry logEntry = await _probe.WaitForWrittenEntryAsync();
        Assert.Equal(typeof(InvalidOperationException).FullName, logEntry.ContextData["ExceptionType"]);
    }

    public void Dispose()
        => _probe.Dispose();

    // Only a thrown exception carries a stack trace.
    private static InvalidOperationException CreateThrownException()
    {
        try
        {
            throw new InvalidOperationException("Outer failure", new ArgumentException("Inner failure"));
        }
        catch (InvalidOperationException exception)
        {
            return exception;
        }
    }

    private static void AssertDescribesThrownException(LogEntry logEntry)
    {
        Assert.Equal("Outer failure", logEntry.Message);
        Assert.Equal(typeof(InvalidOperationException).FullName, logEntry.ContextData["ExceptionType"]);
        Assert.Equal("Inner failure", logEntry.ContextData["InnerException"]);
        Assert.False(string.IsNullOrEmpty(logEntry.ContextData["StackTrace"]));
    }
}
