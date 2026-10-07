namespace GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts.Models;

/// <summary>
/// Settings for dumping the GwiOS database.
/// </summary>
public sealed class DatabaseBackupOptions
{
    /// <summary>
    /// Npgsql connection string of the GwiOS database, including the password. The shared data source cannot provide
    /// it, because it strips the password from its connection string.
    /// </summary>
    public required string ConnectionString { get; init; }
}
