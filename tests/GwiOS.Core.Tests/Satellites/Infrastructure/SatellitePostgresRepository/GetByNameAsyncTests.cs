using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Satellites.Infrastructure.SatellitePostgresRepository;

/// <summary>
/// Covers <c>SatellitePostgresRepository.GetByNameAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class GetByNameAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly ISatelliteRepository _satelliteRepository = database.SatelliteRepository;
    private readonly TestSatellites _testSatellites = new(database.SatelliteRepository);

    [Fact]
    public async Task ReturnsTheSatelliteWithTheName()
    {
        Satellite satellite = _testSatellites.Create();
        await _satelliteRepository.InsertAsync(satellite, TestContext.Current.CancellationToken);

        Satellite? storedSatellite =
            await _satelliteRepository.GetByNameAsync(satellite.Name, TestContext.Current.CancellationToken);

        Assert.Equal(satellite.Name, storedSatellite?.Name);
    }

    [Fact]
    public async Task ReturnsNull_WhenNoSatelliteHasTheName()
    {
        Satellite satellite = _testSatellites.Create();

        Assert.Null(await _satelliteRepository.GetByNameAsync(satellite.Name, TestContext.Current.CancellationToken));
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testSatellites.DisposeAsync();
}
