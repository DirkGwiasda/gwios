using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Satellites.Infrastructure.SatellitePostgresRepository;

/// <summary>
/// Covers <c>SatellitePostgresRepository.EnsureStorageCreatedAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class EnsureStorageCreatedAsyncTests(PostgresTestDatabase database)
{
    [Fact]
    public async Task CanBeCalledAgainOnExistingTables()
    {
        await database.SatelliteRepository.EnsureStorageCreatedAsync(TestContext.Current.CancellationToken);
        await database.SatelliteRepository.EnsureStorageCreatedAsync(TestContext.Current.CancellationToken);
    }
}
