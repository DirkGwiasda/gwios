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
    /// Returns one page of the log entries of all applications, newest first, together with the number of all stored
    /// entries. <paramref name="pageNumber"/> counts from 1; a page beyond the last entry is empty. Throws an
    /// <see cref="ArgumentOutOfRangeException"/> if the page number or the page size is less than 1.
    /// </summary>
    Task<LogEntryPage> GetLogEntryPageAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Returns one page of the log entries of the given application, newest first, together with the number of all
    /// entries stored for it. <paramref name="pageNumber"/> counts from 1; a page beyond the last entry is empty.
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the page number or the page size is less than 1.
    /// </summary>
    Task<LogEntryPage> GetLogEntryPageByAppAsync(string appName, int pageNumber, int pageSize);

    /// <summary>
    /// Deletes all log entries stored for the given application. Does nothing if the application has no entries.
    /// </summary>
    Task DeleteByAppNameAsync(string appName);
}
