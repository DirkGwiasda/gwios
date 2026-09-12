using GwiOS.Core.CrossCutting.Logging.Domain.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

namespace GwiOS.WebUI.Tests.TestInfrastructure;

/// <summary>
/// Stands in for <see cref="ILogEntryManager"/> by keeping log entries in memory. Like the real manager it returns the
/// application names in ascending order and the entries of an application newest first.
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

    public Task<List<string>> GetAllAppNamesAsync()
        => Task.FromResult(
            _entries.Select(entry => entry.AppName).Distinct().Order(StringComparer.Ordinal).ToList());

    public Task<List<LogEntry>> GetAllLogEntriesByAppAsync(string appName)
        => Task.FromResult(
            _entries.Where(entry => entry.AppName == appName).OrderByDescending(entry => entry.Timestamp).ToList());

    public Task DeleteByAppNameAsync(string appName)
    {
        _entries.RemoveAll(entry => entry.AppName == appName);
        return Task.CompletedTask;
    }
}
