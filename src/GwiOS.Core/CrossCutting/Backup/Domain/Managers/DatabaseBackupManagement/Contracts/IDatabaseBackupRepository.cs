using GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts.Exceptions;

namespace GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts;

/// <summary>
/// Dumps the GwiOS database into a file that the standard PostgreSQL tools can restore.
/// </summary>
public interface IDatabaseBackupRepository
{
    /// <summary>
    /// Dumps the structure and data of all schemas, but no data of the schema <c>logging</c>, and returns the dump
    /// as a readable stream. The dump is complete once this method returns. Disposing the stream removes the dump
    /// from the server. Throws a <see cref="DatabaseBackupFailedException"/> if the dump could not be created.
    /// </summary>
    Task<Stream> CreateDumpAsync(CancellationToken cancellationToken = default);
}
