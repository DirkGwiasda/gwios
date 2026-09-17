using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.Contracts;

/// <summary>
/// Stands in for <see cref="ISatelliteRepository"/> by keeping satellites in memory. Like the real repository it
/// orders by name, rejects a name that is already used and an update of a satellite that is not stored.
/// </summary>
public sealed class SatelliteRepositoryFake : ISatelliteRepository
{
    private readonly List<Satellite> _satellites = [];

    /// <summary>
    /// The stored satellites, ordered by name.
    /// </summary>
    public IReadOnlyList<Satellite> Satellites
        => [.. _satellites.OrderBy(satellite => satellite.Name, StringComparer.Ordinal)];

    /// <summary>
    /// Stores the given satellite directly, as if it had been created before the test.
    /// </summary>
    public void Add(Satellite satellite)
        => _satellites.Add(satellite);

    public Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task InsertAsync(Satellite satellite, CancellationToken cancellationToken = default)
    {
        if (_satellites.Any(storedSatellite => storedSatellite.Name == satellite.Name))
        {
            throw new DuplicateSatelliteException(satellite.Name, new InvalidOperationException("Name already used."));
        }

        _satellites.Add(satellite);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Satellite satellite, CancellationToken cancellationToken = default)
    {
        int index = _satellites.FindIndex(storedSatellite => storedSatellite.Name == satellite.Name);
        if (index < 0)
        {
            throw new SatelliteNotFoundException(satellite.Name);
        }

        _satellites[index] = satellite;
        return Task.CompletedTask;
    }

    public Task<List<Satellite>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Satellites.ToList());

    public Task<Satellite?> GetByNameAsync(string satelliteName, CancellationToken cancellationToken = default)
        => Task.FromResult(_satellites.SingleOrDefault(satellite => satellite.Name == satelliteName));

    public Task DeleteAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        _satellites.RemoveAll(satellite => satellite.Name == satelliteName);
        return Task.CompletedTask;
    }
}
