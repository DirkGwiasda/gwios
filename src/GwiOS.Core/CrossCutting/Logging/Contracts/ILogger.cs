namespace GwiOS.Core.CrossCutting.Logging.Contracts;

public interface ILogger<T>
{
    void LogInformation(string message);
    void LogInformation(string appName, string message);
    void LogInformation(string message, Dictionary<string, string> data);
    void LogInformation(string appName, string message, Dictionary<string, string> data);

    void LogError(string message);
    void LogError(string appName, string message);
    void LogError(Exception exception);
    void LogError(string appName, Exception exception);
    void LogError(string message, Dictionary<string, string> data);
    void LogError(string appName, string message, Dictionary<string, string> data);
    void LogError(Exception exception, Dictionary<string, string> data);
    void LogError(string appName, Exception exception, Dictionary<string, string> data);

    void LogWarning(string message);
    void LogWarning(string appName, string message);
    void LogWarning(string message, Dictionary<string, string> data);
    void LogWarning(string appName, string message, Dictionary<string, string> data);

    void LogDebug(string message);
    void LogDebug(string appName, string message);
    void LogDebug(string message, Dictionary<string, string> data);
    void LogDebug(string appName, string message, Dictionary<string, string> data);
}