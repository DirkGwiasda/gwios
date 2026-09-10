using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

namespace GwiOS.Core.CrossCutting.Logging.Domain.Contracts;

/// <summary>
/// Gives access to the stored log entries: lists the applications that logged, reads their entries and deletes them.
/// </summary>
public interface ILogEntryManager
{
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
