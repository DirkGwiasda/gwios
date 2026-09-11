using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

namespace GwiOS.Core.CrossCutting.Logging.Contracts;

/// <summary>
/// Persists, reads and deletes log entries in the underlying storage.
/// </summary>
public interface ILogEntryRepository
{
    /// <summary>
    /// Creates the tables required for log entries unless they already exist. The database itself must already
    /// exist. The operation is idempotent: calling it against existing tables changes nothing.
    /// </summary>
    Task EnsureStorageCreatedAsync();

    /// <summary>
    /// Stores the given log entry.
    /// </summary>
    Task InsertAsync(LogEntry logEntry);

    /// <summary>
    /// Returns the distinct names of all applications that have at least one stored log entry, in ascending
    /// order. The list is empty if no entries are stored.
    /// </summary>
    Task<List<string>> GetAllAppNamesAsync();

    /// <summary>
    /// Returns all log entries stored for the given application, newest first. The list is empty if the
    /// application has no entries.
    /// </summary>
    Task<List<LogEntry>> GetAllLogEntriesByAppAsync(string appName);

    /// <summary>
    /// Deletes all log entries stored for the given application. Does nothing if the application has no entries.
    /// </summary>
    Task DeleteByAppNameAsync(string appName);
}
