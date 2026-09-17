using SatelliteValidationExceptionUnderTest =
    GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions.SatelliteValidationException;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions.SatelliteValidationException;

/// <summary>
/// Covers the constructor of <c>SatelliteValidationException</c>.
/// </summary>
public sealed class ConstructorTests
{
    [Fact]
    public void KeepsTheSatelliteNameAndTheValidationErrors()
    {
        SatelliteValidationExceptionUnderTest exception = new("Keller", ["First.", "Second."]);

        Assert.Equal("Keller", exception.SatelliteName);
        Assert.Equal(["First.", "Second."], exception.ValidationErrors);
    }

    [Fact]
    public void NamesTheSatelliteAndAllErrorsInTheMessage()
    {
        SatelliteValidationExceptionUnderTest exception = new("Keller", ["First.", "Second."]);

        Assert.Equal("Satellite 'Keller' is invalid: First. Second.", exception.Message);
    }
}
