using GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts;
using GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts.Exceptions;
using GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts.Models;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.WebUI.Api.Backup.Contracts;

namespace GwiOS.WebUI.Api.Backup;

internal sealed class DatabaseBackupEndpoint(
    IDatabaseBackupManager databaseBackupManager,
    ILogger<DatabaseBackupEndpoint> logger) : IDatabaseBackupEndpoint
{
    private const string BackupContentType = "application/octet-stream";

    private readonly IDatabaseBackupManager _databaseBackupManager = databaseBackupManager;
    private readonly ILogger<DatabaseBackupEndpoint> _logger = logger;

    public async Task<IResult> HandleAsync(CancellationToken cancellationToken)
    {
        DatabaseBackup backup;
        try
        {
            backup = await _databaseBackupManager.CreateBackupAsync(cancellationToken);
        }
        catch (DatabaseBackupFailedException)
        {
            // The manager has already logged the cause; this only records what the user got to see.
            _logger.LogWarning("Database backup download failed, redirecting to the backup page.");
            return Results.Redirect(IDatabaseBackupEndpoint.FailurePageUrl);
        }

        // The file result disposes the content once it is sent, which also removes the backup from the server.
        return Results.File(backup.Content, BackupContentType, backup.FileName);
    }
}
