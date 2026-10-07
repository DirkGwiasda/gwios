namespace GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when a backup of the database could not be created, e.g. because the database is not reachable or the
/// backup tool is missing.
/// </summary>
/// <param name="message">What failed, including the error output of the backup tool if there was any.</param>
/// <param name="innerException">The exception that caused the failure, or <c>null</c> if there is none.</param>
public sealed class DatabaseBackupFailedException(string message, Exception? innerException = null)
    : Exception(message, innerException);
