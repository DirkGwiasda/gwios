using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Satellites.Infrastructure.SatellitePostgresRepository;

/// <summary>
/// Covers <c>SatellitePostgresRepository.DeleteAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class DeleteAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly ISatelliteRepository _satelliteRepository = database.SatelliteRepository;
    private readonly TestSatellites _testSatellites = new(database.SatelliteRepository);

    [Fact]
    public async Task DeletesOnlyTheSatelliteWithTheName()
    {
        Satellite deleted = _testSatellites.Create();
        Satellite kept = _testSatellites.Create();
        await _satelliteRepository.InsertAsync(deleted, TestContext.Current.CancellationToken);
        await _satelliteRepository.InsertAsync(kept, TestContext.Current.CancellationToken);

        await _satelliteRepository.DeleteAsync(deleted.Name, TestContext.Current.CancellationToken);

        Assert.Null(await _testSatellites.FindStoredAsync(deleted.Name));
        Assert.NotNull(await _testSatellites.FindStoredAsync(kept.Name));
    }

    [Fact]
    public async Task DoesNothing_WhenNoSatelliteHasTheName()
    {
        await _satelliteRepository.DeleteAsync(_testSatellites.Create().Name, TestContext.Current.CancellationToken);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testSatellites.DisposeAsync();
}
