namespace GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts.Models;

/// <summary>
/// A finished backup of the database: its suggested file name and its content. Disposing the backup disposes its
/// content and removes it from the server.
/// </summary>
/// <param name="FileName">The file name to save the backup under, e.g. <c>gwios-backup-2026-10-07_1430.dump</c>.</param>
/// <param name="Content">The backup in the custom format of <c>pg_dump</c>, restorable with <c>pg_restore</c>.</param>
public sealed record DatabaseBackup(string FileName, Stream Content) : IAsyncDisposable
{
    /// <inheritdoc />
    public ValueTask DisposeAsync()
        => Content.DisposeAsync();
}
