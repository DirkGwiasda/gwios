using MeasurementFetchExceptionUnderTest =
    GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Exceptions.MeasurementFetchException;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.MeasurementManagement.Contracts.Exceptions.MeasurementFetchException;

/// <summary>
/// Covers the constructor of <c>MeasurementFetchException</c>.
/// </summary>
public sealed class ConstructorTests
{
    [Fact]
    public void KeepsTheSatelliteNameAndTheInnerException()
    {
        HttpRequestException innerException = new("Connection refused.");

        MeasurementFetchExceptionUnderTest exception = new("keller", "The satellite is not reachable.", innerException);

        Assert.Equal("keller", exception.SatelliteName);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void NamesTheSatelliteAndTheReasonInTheMessage()
    {
        MeasurementFetchExceptionUnderTest exception = new("keller", "The satellite is not reachable.");

        Assert.Equal(
            "The measurements of satellite 'keller' cannot be fetched: The satellite is not reachable.",
            exception.Message);
        Assert.Null(exception.InnerException);
    }
}
