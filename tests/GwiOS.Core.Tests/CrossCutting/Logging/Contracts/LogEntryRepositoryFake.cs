using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

namespace GwiOS.Core.Tests.CrossCutting.Logging.Contracts;

/// <summary>
/// Stands in for <see cref="ILogEntryRepository"/> by keeping log entries in memory. It orders and filters like the
/// real repository and lets a test wait for the first insert, which callers may perform on another thread.
/// </summary>
public sealed class LogEntryRepositoryFake : ILogEntryRepository
{
    private static readonly TimeSpan InsertTimeout = TimeSpan.FromSeconds(5);

    private readonly List<LogEntry> _entries = [];
    private readonly Lock _entriesLock = new();
    private readonly TaskCompletionSource<LogEntry> _firstInsert =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Waits until the first log entry has been inserted and returns it. Fails after five seconds.
    /// </summary>
    public Task<LogEntry> WaitForFirstInsertAsync()
        => _firstInsert.Task.WaitAsync(InsertTimeout);

    public Task EnsureStorageCreatedAsync()
        => Task.CompletedTask;

    public Task InsertAsync(LogEntry logEntry)
    {
        lock (_entriesLock)
        {
            _entries.Add(logEntry);
        }

        _firstInsert.TrySetResult(logEntry);
        return Task.CompletedTask;
    }

    public Task<List<string>> GetAllAppNamesAsync()
    {
        lock (_entriesLock)
        {
            return Task.FromResult(_entries.Select(entry => entry.AppName).Distinct().Order().ToList());
        }
    }

    public Task<List<LogEntry>> GetAllLogEntriesByAppAsync(string appName)
    {
        lock (_entriesLock)
        {
            return Task.FromResult(
                _entries.Where(entry => entry.AppName == appName).OrderByDescending(entry => entry.Timestamp).ToList());
        }
    }

    public Task DeleteByAppNameAsync(string appName)
    {
        lock (_entriesLock)
        {
            _entries.RemoveAll(entry => entry.AppName == appName);
        }

        return Task.CompletedTask;
    }
}
