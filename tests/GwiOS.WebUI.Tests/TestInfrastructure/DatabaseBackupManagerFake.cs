using GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts;
using GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts.Models;

namespace GwiOS.WebUI.Tests.TestInfrastructure;

/// <summary>
/// Stands in for <see cref="IDatabaseBackupManager"/>: returns a backup named <see cref="FileName"/> with
/// <see cref="Content"/>, or throws <see cref="ExceptionToThrow"/> if a test sets it.
/// </summary>
public sealed class DatabaseBackupManagerFake : IDatabaseBackupManager
{
    /// <summary>
    /// The file name of every backup.
    /// </summary>
    public string FileName { get; set; } = "gwios-backup-2026-10-07_1430.dump";

    /// <summary>
    /// The bytes every backup consists of.
    /// </summary>
    public byte[] Content { get; set; } = "PGDMP"u8.ToArray();

    /// <summary>
    /// The exception creating a backup throws, or <c>null</c> to succeed.
    /// </summary>
    public Exception? ExceptionToThrow { get; set; }

    public Task<DatabaseBackup> CreateBackupAsync(CancellationToken cancellationToken = default)
        => ExceptionToThrow is null
            ? Task.FromResult(new DatabaseBackup(FileName, new MemoryStream(Content)))
            : Task.FromException<DatabaseBackup>(ExceptionToThrow);
}
