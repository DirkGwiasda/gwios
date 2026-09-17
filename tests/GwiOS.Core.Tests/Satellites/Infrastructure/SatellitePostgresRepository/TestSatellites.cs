using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Infrastructure.SatellitePostgresRepository;

/// <summary>
/// Creates satellites with unique names for the tests against the PostgreSQL test database and deletes every satellite
/// it created when it is disposed. The database may contain satellites of other tests, so tests only look at the
/// satellites they created themselves.
/// </summary>
public sealed class TestSatellites(ISatelliteRepository satelliteRepository) : IAsyncDisposable
{
    private readonly ISatelliteRepository _satelliteRepository = satelliteRepository;
    private readonly List<string> _names = [];

    /// <summary>
    /// Returns a new, not yet stored satellite with a unique name, a display name and a token.
    /// </summary>
    public Satellite Create()
    {
        Satellite satellite = new()
        {
            Name = $"test-{Guid.NewGuid():N}"[..32],
            DisplayName = "Testsatellit",
            ProtectedToken = "protected-token",
            CreatedAt = new DateTime(2026, 9, 17, 10, 30, 0, DateTimeKind.Utc)
        };
        _names.Add(satellite.Name);
        return satellite;
    }

    /// <summary>
    /// Returns the stored satellite with the given name, or <c>null</c> if no such satellite is stored.
    /// </summary>
    public Task<Satellite?> FindStoredAsync(string name)
        => _satelliteRepository.GetByNameAsync(name);

    public async ValueTask DisposeAsync()
    {
        foreach (string name in _names)
        {
            await _satelliteRepository.DeleteAsync(name);
        }
    }
}
