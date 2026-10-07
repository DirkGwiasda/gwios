using GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts;
using GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts.Exceptions;
using GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts.Models;
using GwiOS.Core.CrossCutting.Logging.Contracts;

namespace GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement;

internal sealed class DatabaseBackupManager(
    IDatabaseBackupRepository databaseBackupRepository,
    TimeProvider timeProvider,
    ILogger<DatabaseBackupManager> logger) : IDatabaseBackupManager
{
    private readonly IDatabaseBackupRepository _databaseBackupRepository = databaseBackupRepository;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<DatabaseBackupManager> _logger = logger;

    public async Task<DatabaseBackup> CreateBackupAsync(CancellationToken cancellationToken = default)
    {
        string fileName = CreateFileName();
        Stream content;
        try
        {
            content = await _databaseBackupRepository.CreateDumpAsync(cancellationToken);
        }
        catch (DatabaseBackupFailedException exception)
        {
            _logger.LogError(exception, new Dictionary<string, string> { ["FileName"] = fileName });
            throw;
        }

        _logger.LogInformation(
            "Database backup created.",
            new Dictionary<string, string> { ["FileName"] = fileName, ["Bytes"] = content.Length.ToString() });
        return new DatabaseBackup(fileName, content);
    }

    // Local time, because the family reads the name; minutes are enough to tell backups of one day apart.
    private string CreateFileName()
        => $"gwios-backup-{_timeProvider.GetLocalNow():yyyy-MM-dd_HHmm}.dump";
}
