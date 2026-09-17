using GwiOS.Core.CrossCutting.Logging.Domain.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

namespace GwiOS.WebUI.Tests.TestInfrastructure;

/// <summary>
/// Stands in for <see cref="ILogEntryManager"/> by keeping log entries in memory. Like the real manager it returns the
/// application names in ascending order and the entries page by page, newest first.
/// </summary>
public sealed class LogEntryManagerFake : ILogEntryManager
{
    private readonly List<LogEntry> _entries = [];

    /// <summary>
    /// Stores the given entry directly, as if it had been written before the test.
    /// </summary>
    public void Add(LogEntry entry)
        => _entries.Add(entry);

    /// <summary>
    /// The number of stored entries of the given application.
    /// </summary>
    public int CountEntries(string appName)
        => _entries.Count(entry => entry.AppName == appName);

    /// <summary>
    /// Deletes all stored entries of the given application directly, as if someone else had deleted them.
    /// </summary>
    public void Remove(string appName)
        => _entries.RemoveAll(entry => entry.AppName == appName);

    public Task<List<string>> GetAllAppNamesAsync()
        => Task.FromResult(
            _entries.Select(entry => entry.AppName).Distinct().Order(StringComparer.Ordinal).ToList());

    public Task<LogEntryPage> GetLogEntryPageAsync(int pageNumber, int pageSize)
        => Task.FromResult(CreatePage(_entries, pageNumber, pageSize));

    public Task<LogEntryPage> GetLogEntryPageByAppAsync(string appName, int pageNumber, int pageSize)
        => Task.FromResult(CreatePage([.. _entries.Where(entry => entry.AppName == appName)], pageNumber, pageSize));

    public Task DeleteByAppNameAsync(string appName)
    {
        Remove(appName);
        return Task.CompletedTask;
    }

    private static LogEntryPage CreatePage(List<LogEntry> entries, int pageNumber, int pageSize)
        => new(
            [.. entries
                .OrderByDescending(entry => entry.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)],
            entries.Count);
}
