using SatelliteUnderTest = GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models.Satellite;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models.Satellite;

/// <summary>
/// Covers the defaults of a new <c>Satellite</c>.
/// </summary>
public sealed class ConstructorTests
{
    [Fact]
    public void CreatesAnUnlockedSatelliteWithoutDisplayNameAndToken()
    {
        SatelliteUnderTest satellite = new() { Name = "keller" };

        Assert.False(satellite.IsLocked);
        Assert.Null(satellite.DisplayName);
        Assert.Null(satellite.ProtectedToken);
    }

    [Fact]
    public void StampsTheCreationTimeInUtc()
    {
        DateTime before = DateTime.UtcNow;

        SatelliteUnderTest satellite = new() { Name = "keller" };

        Assert.InRange(satellite.CreatedAt, before, DateTime.UtcNow);
        Assert.Equal(DateTimeKind.Utc, satellite.CreatedAt.Kind);
    }
}
