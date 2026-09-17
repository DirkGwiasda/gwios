using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Satellites.Infrastructure.SatellitePostgresRepository;

/// <summary>
/// Covers <c>SatellitePostgresRepository.GetAllAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class GetAllAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly ISatelliteRepository _satelliteRepository = database.SatelliteRepository;
    private readonly TestSatellites _testSatellites = new(database.SatelliteRepository);

    [Fact]
    public async Task ReturnsTheStoredSatellitesOrderedByName()
    {
        Satellite first = _testSatellites.Create();
        Satellite second = _testSatellites.Create();
        await _satelliteRepository.InsertAsync(second, TestContext.Current.CancellationToken);
        await _satelliteRepository.InsertAsync(first, TestContext.Current.CancellationToken);
        string[] expectedNames = [.. new[] { first.Name, second.Name }.Order(StringComparer.Ordinal)];

        List<Satellite> satellites = await _satelliteRepository.GetAllAsync(TestContext.Current.CancellationToken);

        Assert.Equal(expectedNames, satellites.Select(satellite => satellite.Name).Where(expectedNames.Contains));
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testSatellites.DisposeAsync();
}
