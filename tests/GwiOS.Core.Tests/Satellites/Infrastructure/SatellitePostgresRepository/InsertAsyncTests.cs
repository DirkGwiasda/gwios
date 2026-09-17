using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Satellites.Infrastructure.SatellitePostgresRepository;

/// <summary>
/// Covers <c>SatellitePostgresRepository.InsertAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class InsertAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly PostgresTestDatabase _database = database;
    private readonly ISatelliteRepository _satelliteRepository = database.SatelliteRepository;
    private readonly TestSatellites _testSatellites = new(database.SatelliteRepository);

    [Fact]
    public async Task StoresAllPropertiesOfTheSatellite()
    {
        Satellite satellite = _testSatellites.Create();
        satellite.IsLocked = true;

        await _satelliteRepository.InsertAsync(satellite, TestContext.Current.CancellationToken);

        Satellite? storedSatellite = await _testSatellites.FindStoredAsync(satellite.Name);
        Assert.NotNull(storedSatellite);
        Assert.Equal(satellite.DisplayName, storedSatellite.DisplayName);
        Assert.True(storedSatellite.IsLocked);
        Assert.Equal(satellite.ProtectedToken, storedSatellite.ProtectedToken);
        Assert.Equal(satellite.CreatedAt, storedSatellite.CreatedAt);
    }

    [Fact]
    public async Task StoresASatelliteWithoutDisplayNameAndToken()
    {
        Satellite satellite = _testSatellites.Create();
        satellite.DisplayName = null;
        satellite.ProtectedToken = null;

        await _satelliteRepository.InsertAsync(satellite, TestContext.Current.CancellationToken);

        Satellite? storedSatellite = await _testSatellites.FindStoredAsync(satellite.Name);
        Assert.NotNull(storedSatellite);
        Assert.Null(storedSatellite.DisplayName);
        Assert.Null(storedSatellite.ProtectedToken);
    }

    [Fact]
    public async Task ThrowsDuplicateSatelliteExceptionAndLogsAWarning_WhenTheNameIsAlreadyUsed()
    {
        Satellite satellite = _testSatellites.Create();
        await _satelliteRepository.InsertAsync(satellite, TestContext.Current.CancellationToken);

        DuplicateSatelliteException exception = await Assert.ThrowsAsync<DuplicateSatelliteException>(
            () => _satelliteRepository.InsertAsync(
                new Satellite { Name = satellite.Name },
                TestContext.Current.CancellationToken));

        Assert.Equal(satellite.Name, exception.SatelliteName);
        Assert.Contains(
            _database.SatelliteRepositoryLogger.Entries,
            logEntry => (logEntry.LogLevel == LogLevel.Warning)
                && (logEntry.ContextData.GetValueOrDefault("SatelliteName") == satellite.Name));
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testSatellites.DisposeAsync();
}
