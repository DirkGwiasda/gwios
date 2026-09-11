using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using Microsoft.Extensions.DependencyInjection;

namespace GwiOS.Core.CrossCutting.Logging;

internal sealed class DefaultLogger<T>(IServiceScopeFactory scopeFactory) : ILogger<T>
{
    private static readonly string DefaultAppName = "GwiOS";

    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    public void LogDebug(string message)
        => Log(DefaultAppName, message, LogLevel.Debug);

    public void LogDebug(string appName, string message)
        => Log(appName, message, LogLevel.Debug);

    public void LogDebug(string message, Dictionary<string, string> data)
        => Log(DefaultAppName, message, LogLevel.Debug, data);

    public void LogDebug(string appName, string message, Dictionary<string, string> data)
        => Log(appName, message, LogLevel.Debug, data);

    public void LogError(string message)
        => Log(DefaultAppName, message, LogLevel.Error);

    public void LogError(string appName, string message)
        => Log(appName, message, LogLevel.Error);

    public void LogError(Exception exception)
        => Log(DefaultAppName, exception.Message, LogLevel.Error, BuildExceptionData(exception));

    public void LogError(string appName, Exception exception)
        => Log(appName, exception.Message, LogLevel.Error, BuildExceptionData(exception));

    public void LogError(string message, Dictionary<string, string> data)
        => Log(DefaultAppName, message, LogLevel.Error, data);

    public void LogError(string appName, string message, Dictionary<string, string> data)
        => Log(appName, message, LogLevel.Error, data);

    public void LogError(Exception exception, Dictionary<string, string> data)
        => Log(DefaultAppName, exception.Message, LogLevel.Error, MergeExceptionData(exception, data));

    public void LogError(string appName, Exception exception, Dictionary<string, string> data)
        => Log(appName, exception.Message, LogLevel.Error, MergeExceptionData(exception, data));

    public void LogInformation(string message)
        => Log(DefaultAppName, message, LogLevel.Information);

    public void LogInformation(string appName, string message)
        => Log(appName, message, LogLevel.Information);

    public void LogInformation(string message, Dictionary<string, string> data)
        => Log(DefaultAppName, message, LogLevel.Information, data);

    public void LogInformation(string appName, string message, Dictionary<string, string> data)
        => Log(appName, message, LogLevel.Information, data);

    public void LogWarning(string message)
        => Log(DefaultAppName, message, LogLevel.Warning);

    public void LogWarning(string appName, string message)
        => Log(appName, message, LogLevel.Warning);

    public void LogWarning(string message, Dictionary<string, string> data)
        => Log(DefaultAppName, message, LogLevel.Warning, data);

    public void LogWarning(string appName, string message, Dictionary<string, string> data)
        => Log(appName, message, LogLevel.Warning, data);

    private void Log(string appName, string message, LogLevel logLevel, Dictionary<string, string>? data = null)
    {
        var logEntry = new LogEntry(appName, typeof(T).FullName ?? typeof(T).Name, message, logLevel, data);
        _ = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ILogEntryRepository>();
            await repository.InsertAsync(logEntry);
        });
    }

    private static Dictionary<string, string> BuildExceptionData(Exception exception)
    {
        var data = new Dictionary<string, string>
        {
            ["ExceptionType"] = exception.GetType().FullName ?? exception.GetType().Name
        };

        if (exception.StackTrace is not null)
        {
            data["StackTrace"] = exception.StackTrace;
        }

        if (exception.InnerException is not null)
        {
            data["InnerException"] = exception.InnerException.Message;
        }

        return data;
    }

    private static Dictionary<string, string> MergeExceptionData(Exception exception, Dictionary<string, string> data)
    {
        var exceptionData = BuildExceptionData(exception);

        foreach (var kvp in data)
        {
            exceptionData.TryAdd(kvp.Key, kvp.Value);
        }

        return exceptionData;
    }
}

