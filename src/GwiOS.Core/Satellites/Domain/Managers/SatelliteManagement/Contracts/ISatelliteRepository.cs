using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;

/// <summary>
/// Persists, reads and deletes satellites in the underlying storage.
/// </summary>
public interface ISatelliteRepository
{
    /// <summary>
    /// Creates the tables required for satellites unless they already exist. The database itself must already exist.
    /// The operation is idempotent: calling it against existing tables changes nothing.
    /// </summary>
    Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores the given new satellite. Throws a <see cref="DuplicateSatelliteException"/> if its name is already used.
    /// </summary>
    Task InsertAsync(Satellite satellite, CancellationToken cancellationToken = default);

    /// <summary>
    /// Overwrites display name, lock state and token of the stored satellite with the name of the given satellite.
    /// Throws a <see cref="SatelliteNotFoundException"/> if no such satellite is stored.
    /// </summary>
    Task UpdateAsync(Satellite satellite, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all stored satellites, ordered by name. The list is empty if no satellites are stored.
    /// </summary>
    Task<List<Satellite>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the stored satellite with the given name, or <c>null</c> if no such satellite is stored.
    /// </summary>
    Task<Satellite?> GetByNameAsync(string satelliteName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the satellite with the given name. Does nothing if no such satellite is stored.
    /// </summary>
    Task DeleteAsync(string satelliteName, CancellationToken cancellationToken = default);
}
