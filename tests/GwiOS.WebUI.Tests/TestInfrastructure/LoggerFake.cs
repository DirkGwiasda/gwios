using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

namespace GwiOS.WebUI.Tests.TestInfrastructure;

/// <summary>
/// Stands in for <see cref="ILogger{T}"/> by recording every written entry in memory, synchronously and in order.
/// Overloads without an application name record <see cref="DefaultAppName"/>; overloads with an exception record
/// its message and only the given data, not the details of the exception.
/// </summary>
/// <typeparam name="T">The type that writes the entries; it is recorded as their data source.</typeparam>
public sealed class LoggerFake<T> : ILogger<T>
{
    /// <summary>
    /// The application name recorded by overloads that take no application name.
    /// </summary>
    public const string DefaultAppName = "Default";

    private readonly List<LogEntry> _entries = [];

    /// <summary>
    /// All entries written so far, oldest first. Empty if nothing has been written.
    /// </summary>
    public IReadOnlyList<LogEntry> Entries
        => _entries;

    public void LogInformation(string message)
        => Record(DefaultAppName, message, LogLevel.Information);

    public void LogInformation(string appName, string message)
        => Record(appName, message, LogLevel.Information);

    public void LogInformation(string message, Dictionary<string, string> data)
        => Record(DefaultAppName, message, LogLevel.Information, data);

    public void LogInformation(string appName, string message, Dictionary<string, string> data)
        => Record(appName, message, LogLevel.Information, data);

    public void LogError(string message)
        => Record(DefaultAppName, message, LogLevel.Error);

    public void LogError(string appName, string message)
        => Record(appName, message, LogLevel.Error);

    public void LogError(Exception exception)
        => Record(DefaultAppName, exception.Message, LogLevel.Error);

    public void LogError(string appName, Exception exception)
        => Record(appName, exception.Message, LogLevel.Error);

    public void LogError(string message, Dictionary<string, string> data)
        => Record(DefaultAppName, message, LogLevel.Error, data);

    public void LogError(string appName, string message, Dictionary<string, string> data)
        => Record(appName, message, LogLevel.Error, data);

    public void LogError(Exception exception, Dictionary<string, string> data)
        => Record(DefaultAppName, exception.Message, LogLevel.Error, data);

    public void LogError(string appName, Exception exception, Dictionary<string, string> data)
        => Record(appName, exception.Message, LogLevel.Error, data);

    public void LogWarning(string message)
        => Record(DefaultAppName, message, LogLevel.Warning);

    public void LogWarning(string appName, string message)
        => Record(appName, message, LogLevel.Warning);

    public void LogWarning(string message, Dictionary<string, string> data)
        => Record(DefaultAppName, message, LogLevel.Warning, data);

    public void LogWarning(string appName, string message, Dictionary<string, string> data)
        => Record(appName, message, LogLevel.Warning, data);

    public void LogDebug(string message)
        => Record(DefaultAppName, message, LogLevel.Debug);

    public void LogDebug(string appName, string message)
        => Record(appName, message, LogLevel.Debug);

    public void LogDebug(string message, Dictionary<string, string> data)
        => Record(DefaultAppName, message, LogLevel.Debug, data);

    public void LogDebug(string appName, string message, Dictionary<string, string> data)
        => Record(appName, message, LogLevel.Debug, data);

    private void Record(string appName, string message, LogLevel logLevel, Dictionary<string, string>? data = null)
        => _entries.Add(new LogEntry(appName, typeof(T).FullName ?? typeof(T).Name, message, logLevel, data));
}
