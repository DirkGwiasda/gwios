using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Satellites.Infrastructure.SatellitePostgresRepository;

/// <summary>
/// Covers <c>SatellitePostgresRepository.UpdateAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class UpdateAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly ISatelliteRepository _satelliteRepository = database.SatelliteRepository;
    private readonly TestSatellites _testSatellites = new(database.SatelliteRepository);

    [Fact]
    public async Task OverwritesDisplayNameLockStateAndTokenButNotTheCreationTime()
    {
        Satellite satellite = _testSatellites.Create();
        await _satelliteRepository.InsertAsync(satellite, TestContext.Current.CancellationToken);
        Satellite changedSatellite = new()
        {
            Name = satellite.Name,
            DisplayName = "Anderer Name",
            IsLocked = true,
            ProtectedToken = "other-protected-token",
            CreatedAt = satellite.CreatedAt.AddDays(1)
        };

        await _satelliteRepository.UpdateAsync(changedSatellite, TestContext.Current.CancellationToken);

        Satellite? storedSatellite = await _testSatellites.FindStoredAsync(satellite.Name);
        Assert.NotNull(storedSatellite);
        Assert.Equal("Anderer Name", storedSatellite.DisplayName);
        Assert.True(storedSatellite.IsLocked);
        Assert.Equal("other-protected-token", storedSatellite.ProtectedToken);
        Assert.Equal(satellite.CreatedAt, storedSatellite.CreatedAt);
    }

    [Fact]
    public async Task ThrowsSatelliteNotFoundException_WhenTheSatelliteIsNotStored()
    {
        Satellite satellite = _testSatellites.Create();

        await Assert.ThrowsAsync<SatelliteNotFoundException>(
            () => _satelliteRepository.UpdateAsync(satellite, TestContext.Current.CancellationToken));
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testSatellites.DisposeAsync();
}
