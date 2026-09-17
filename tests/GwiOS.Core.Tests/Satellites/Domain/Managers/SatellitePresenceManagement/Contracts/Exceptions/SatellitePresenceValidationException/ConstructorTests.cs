using SatellitePresenceValidationExceptionUnderTest =
    GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Exceptions.SatellitePresenceValidationException;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Exceptions.SatellitePresenceValidationException;

/// <summary>
/// Covers the constructor of <c>SatellitePresenceValidationException</c>.
/// </summary>
public sealed class ConstructorTests
{
    [Fact]
    public void KeepsTheSatelliteNameAndTheValidationErrors()
    {
        SatellitePresenceValidationExceptionUnderTest exception = new("keller", ["First.", "Second."]);

        Assert.Equal("keller", exception.SatelliteName);
        Assert.Equal(["First.", "Second."], exception.ValidationErrors);
    }

    [Fact]
    public void NamesTheSatelliteAndAllErrorsInTheMessage()
    {
        SatellitePresenceValidationExceptionUnderTest exception = new("keller", ["First.", "Second."]);

        Assert.Equal("The ping of satellite 'keller' is invalid: First. Second.", exception.Message);
    }
}
