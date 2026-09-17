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
    /// Returns one page of the log entries of all applications, newest first, together with the number of all stored
    /// entries. <paramref name="pageNumber"/> counts from 1 and <paramref name="pageSize"/> is at least 1; a page
    /// beyond the last entry is empty.
    /// </summary>
    Task<LogEntryPage> GetLogEntryPageAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Returns one page of the log entries of the given application, newest first, together with the number of all
    /// entries stored for it. <paramref name="pageNumber"/> counts from 1 and <paramref name="pageSize"/> is at least
    /// 1; a page beyond the last entry is empty.
    /// </summary>
    Task<LogEntryPage> GetLogEntryPageByAppAsync(string appName, int pageNumber, int pageSize);

    /// <summary>
    /// Deletes all log entries stored for the given application. Does nothing if the application has no entries.
    /// </summary>
    Task DeleteByAppNameAsync(string appName);
}
