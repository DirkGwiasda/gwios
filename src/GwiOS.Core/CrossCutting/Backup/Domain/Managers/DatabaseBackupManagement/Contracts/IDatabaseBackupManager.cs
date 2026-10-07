using GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts.Exceptions;
using GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts.Models;

namespace GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts;

/// <summary>
/// Creates backups of the GwiOS database, e.g. to download and keep them outside the server.
/// </summary>
public interface IDatabaseBackupManager
{
    /// <summary>
    /// Creates a backup of the whole GwiOS database except the data of the log, named after the current local time.
    /// The caller owns the returned backup and must dispose it once its content is read. Throws a
    /// <see cref="DatabaseBackupFailedException"/> if the backup could not be created.
    /// </summary>
    Task<DatabaseBackup> CreateBackupAsync(CancellationToken cancellationToken = default);
}
