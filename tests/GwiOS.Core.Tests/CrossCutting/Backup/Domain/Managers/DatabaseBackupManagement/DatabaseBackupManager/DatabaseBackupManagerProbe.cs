using GwiOS.Core.Tests.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.TestInfrastructure;
using DatabaseBackupManagerUnderTest = GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.DatabaseBackupManager;

namespace GwiOS.Core.Tests.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.DatabaseBackupManager;

/// <summary>
/// Runs a <c>DatabaseBackupManager</c> against a configurable repository, a clock that stands still and a recording
/// logger, and gives the tests access to all three.
/// </summary>
internal sealed class DatabaseBackupManagerProbe
{
    public DatabaseBackupManagerProbe()
    {
        DatabaseBackupManager = new DatabaseBackupManagerUnderTest(DatabaseBackupRepository, TimeProvider, Logger);
    }

    /// <summary>
    /// The repository the manager takes its dumps from.
    /// </summary>
    public DatabaseBackupRepositoryFake DatabaseBackupRepository { get; } = new();

    /// <summary>
    /// The clock the manager names backups after.
    /// </summary>
    public TimeProviderFake TimeProvider { get; } = new();

    /// <summary>
    /// The logger the manager writes to.
    /// </summary>
    public LoggerFake<DatabaseBackupManagerUnderTest> Logger { get; } = new();

    /// <summary>
    /// The manager under test.
    /// </summary>
    public DatabaseBackupManagerUnderTest DatabaseBackupManager { get; }
}
