using GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts;

namespace GwiOS.Core.Tests.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts;

/// <summary>
/// Stands in for <see cref="IDatabaseBackupRepository"/>: returns <see cref="DumpContent"/> as dump, or throws
/// <see cref="ExceptionToThrow"/> if a test sets it.
/// </summary>
public sealed class DatabaseBackupRepositoryFake : IDatabaseBackupRepository
{
    /// <summary>
    /// The bytes every dump consists of.
    /// </summary>
    public byte[] DumpContent { get; set; } = "PGDMP"u8.ToArray();

    /// <summary>
    /// The exception creating a dump throws, or <c>null</c> to succeed.
    /// </summary>
    public Exception? ExceptionToThrow { get; set; }

    public Task<Stream> CreateDumpAsync(CancellationToken cancellationToken = default)
        => ExceptionToThrow is null
            ? Task.FromResult<Stream>(new MemoryStream(DumpContent))
            : Task.FromException<Stream>(ExceptionToThrow);
}
