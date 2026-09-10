namespace GwiOS.Core.CrossCutting.Logging.Contracts;

/// <summary>
/// Writes log entries on behalf of <typeparamref name="T"/>. Overloads without an application name write for the
/// default application; overloads with <c>data</c> attach it to the entry as additional context data.
/// </summary>
/// <typeparam name="T">The type that writes the entries; it is recorded as their data source.</typeparam>
public interface ILogger<T>
{
    /// <summary>
    /// Writes an entry with level <c>Information</c> for the default application.
    /// </summary>
    void LogInformation(string message);

    /// <summary>
    /// Writes an entry with level <c>Information</c> for the given application.
    /// </summary>
    void LogInformation(string appName, string message);

    /// <summary>
    /// Writes an entry with level <c>Information</c> and additional context data for the default application.
    /// </summary>
    void LogInformation(string message, Dictionary<string, string> data);

    /// <summary>
    /// Writes an entry with level <c>Information</c> and additional context data for the given application.
    /// </summary>
    void LogInformation(string appName, string message, Dictionary<string, string> data);

    /// <summary>
    /// Writes an entry with level <c>Error</c> for the default application.
    /// </summary>
    void LogError(string message);

    /// <summary>
    /// Writes an entry with level <c>Error</c> for the given application.
    /// </summary>
    void LogError(string appName, string message);

    /// <summary>
    /// Writes an entry with level <c>Error</c> for the default application. The message is taken from the
    /// exception, and the context data describes the exception.
    /// </summary>
    void LogError(Exception exception);

    /// <summary>
    /// Writes an entry with level <c>Error</c> for the given application. The message is taken from the
    /// exception, and the context data describes the exception.
    /// </summary>
    void LogError(string appName, Exception exception);

    /// <summary>
    /// Writes an entry with level <c>Error</c> and additional context data for the default application.
    /// </summary>
    void LogError(string message, Dictionary<string, string> data);

    /// <summary>
    /// Writes an entry with level <c>Error</c> and additional context data for the given application.
    /// </summary>
    void LogError(string appName, string message, Dictionary<string, string> data);

    /// <summary>
    /// Writes an entry with level <c>Error</c> for the default application. The message is taken from the
    /// exception, and the context data combines the details of the exception with the given data.
    /// </summary>
    void LogError(Exception exception, Dictionary<string, string> data);

    /// <summary>
    /// Writes an entry with level <c>Error</c> for the given application. The message is taken from the
    /// exception, and the context data combines the details of the exception with the given data.
    /// </summary>
    void LogError(string appName, Exception exception, Dictionary<string, string> data);

    /// <summary>
    /// Writes an entry with level <c>Warning</c> for the default application.
    /// </summary>
    void LogWarning(string message);

    /// <summary>
    /// Writes an entry with level <c>Warning</c> for the given application.
    /// </summary>
    void LogWarning(string appName, string message);

    /// <summary>
    /// Writes an entry with level <c>Warning</c> and additional context data for the default application.
    /// </summary>
    void LogWarning(string message, Dictionary<string, string> data);

    /// <summary>
    /// Writes an entry with level <c>Warning</c> and additional context data for the given application.
    /// </summary>
    void LogWarning(string appName, string message, Dictionary<string, string> data);

    /// <summary>
    /// Writes an entry with level <c>Debug</c> for the default application.
    /// </summary>
    void LogDebug(string message);

    /// <summary>
    /// Writes an entry with level <c>Debug</c> for the given application.
    /// </summary>
    void LogDebug(string appName, string message);

    /// <summary>
    /// Writes an entry with level <c>Debug</c> and additional context data for the default application.
    /// </summary>
    void LogDebug(string message, Dictionary<string, string> data);

    /// <summary>
    /// Writes an entry with level <c>Debug</c> and additional context data for the given application.
    /// </summary>
    void LogDebug(string appName, string message, Dictionary<string, string> data);
}
