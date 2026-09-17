using SatelliteNotFoundExceptionUnderTest =
    GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions.SatelliteNotFoundException;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions.SatelliteNotFoundException;

/// <summary>
/// Covers the constructor of <c>SatelliteNotFoundException</c>.
/// </summary>
public sealed class ConstructorTests
{
    [Fact]
    public void KeepsTheSatelliteName()
    {
        SatelliteNotFoundExceptionUnderTest exception = new("keller");

        Assert.Equal("keller", exception.SatelliteName);
    }

    [Fact]
    public void NamesTheSatelliteInTheMessage()
    {
        SatelliteNotFoundExceptionUnderTest exception = new("keller");

        Assert.Equal("No satellite named 'keller' is stored.", exception.Message);
    }
}
