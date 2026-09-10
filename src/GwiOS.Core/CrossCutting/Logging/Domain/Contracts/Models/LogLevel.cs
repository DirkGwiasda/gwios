namespace GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

/// <summary>
/// Severity of a log entry, in ascending order. The numeric values are persisted and must not change.
/// </summary>
public enum LogLevel
{
    None = 0,

    /// <summary>
    /// Diagnostic information intended for development and troubleshooting.
    /// </summary>
    Debug = 1,

    /// <summary>
    /// Information about the normal operation of the application.
    /// </summary>
    Information = 2,

    /// <summary>
    /// An abnormal or unexpected situation that does not constitute an error.
    /// </summary>
    Warning = 3,

    /// <summary>
    /// An error, such as a failed operation or a caught exception.
    /// </summary>
    Error = 4
}
