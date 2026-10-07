using GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts.Exceptions;
using GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts.Models;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

namespace GwiOS.Core.Tests.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.DatabaseBackupManager;

/// <summary>
/// Covers <c>DatabaseBackupManager.CreateBackupAsync</c>.
/// </summary>
public sealed class CreateBackupAsyncTests
{
    private readonly DatabaseBackupManagerProbe _probe = new();

    [Fact]
    public async Task ReturnsTheDumpOfTheRepositoryAsContent()
    {
        _probe.DatabaseBackupRepository.DumpContent = [1, 2, 3];

        await using DatabaseBackup backup = await _probe.DatabaseBackupManager.CreateBackupAsync(
            TestContext.Current.CancellationToken);

        using MemoryStream content = new();
        await backup.Content.CopyToAsync(content, TestContext.Current.CancellationToken);
        Assert.Equal([1, 2, 3], content.ToArray());
    }

    [Fact]
    public async Task NamesTheBackupAfterTheCurrentLocalTime()
    {
        _probe.TimeProvider.UtcNow = new DateTimeOffset(2026, 10, 7, 14, 30, 59, TimeSpan.Zero);

        await using DatabaseBackup backup = await _probe.DatabaseBackupManager.CreateBackupAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal("gwios-backup-2026-10-07_1430.dump", backup.FileName);
    }

    [Fact]
    public async Task LogsTheBackupWithItsFileNameAndSize()
    {
        _probe.DatabaseBackupRepository.DumpContent = [1, 2, 3];

        await using DatabaseBackup backup = await _probe.DatabaseBackupManager.CreateBackupAsync(
            TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Database backup created.", logEntry.Message);
        Assert.Equal(backup.FileName, logEntry.ContextData["FileName"]);
        Assert.Equal("3", logEntry.ContextData["Bytes"]);
    }

    [Fact]
    public async Task RethrowsTheFailureOfTheRepository()
    {
        DatabaseBackupFailedException failure = new("pg_dump exited with code 1: connection refused");
        _probe.DatabaseBackupRepository.ExceptionToThrow = failure;

        DatabaseBackupFailedException exception = await Assert.ThrowsAsync<DatabaseBackupFailedException>(
            () => _probe.DatabaseBackupManager.CreateBackupAsync(TestContext.Current.CancellationToken));

        Assert.Same(failure, exception);
    }

    [Fact]
    public async Task LogsTheFailureAsError()
    {
        _probe.DatabaseBackupRepository.ExceptionToThrow =
            new DatabaseBackupFailedException("pg_dump exited with code 1: connection refused");

        await Assert.ThrowsAsync<DatabaseBackupFailedException>(
            () => _probe.DatabaseBackupManager.CreateBackupAsync(TestContext.Current.CancellationToken));

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Error, logEntry.LogLevel);
        Assert.Equal("pg_dump exited with code 1: connection refused", logEntry.Message);
    }
}
