using SatelliteTokenRejectedExceptionUnderTest =
    GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions.SatelliteTokenRejectedException;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions.SatelliteTokenRejectedException;

/// <summary>
/// Covers the constructor of <c>SatelliteTokenRejectedException</c>.
/// </summary>
public sealed class ConstructorTests
{
    [Fact]
    public void KeepsTheSatelliteName()
    {
        SatelliteTokenRejectedExceptionUnderTest exception = new("keller");

        Assert.Equal("keller", exception.SatelliteName);
    }

    [Fact]
    public void NamesTheSatelliteInTheMessage()
    {
        SatelliteTokenRejectedExceptionUnderTest exception = new("keller");

        Assert.Equal("The token of satellite 'keller' is rejected.", exception.Message);
    }
}
