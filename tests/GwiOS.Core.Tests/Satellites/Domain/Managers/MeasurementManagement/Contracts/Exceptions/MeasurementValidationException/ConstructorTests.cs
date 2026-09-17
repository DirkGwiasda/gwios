using MeasurementValidationExceptionUnderTest =
    GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Exceptions.MeasurementValidationException;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.MeasurementManagement.Contracts.Exceptions.MeasurementValidationException;

/// <summary>
/// Covers the constructor of <c>MeasurementValidationException</c>.
/// </summary>
public sealed class ConstructorTests
{
    [Fact]
    public void KeepsTheSatelliteNameAndTheValidationErrors()
    {
        MeasurementValidationExceptionUnderTest exception = new("keller", ["First.", "Second."]);

        Assert.Equal("keller", exception.SatelliteName);
        Assert.Equal(["First.", "Second."], exception.ValidationErrors);
    }

    [Fact]
    public void NamesTheSatelliteAndAllErrorsInTheMessage()
    {
        MeasurementValidationExceptionUnderTest exception = new("keller", ["First.", "Second."]);

        Assert.Equal("The measurements of satellite 'keller' are invalid: First. Second.", exception.Message);
    }
}
