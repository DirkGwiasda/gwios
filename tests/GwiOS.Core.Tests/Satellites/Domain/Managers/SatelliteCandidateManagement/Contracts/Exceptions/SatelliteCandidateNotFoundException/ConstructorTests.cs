using SatelliteCandidateNotFoundExceptionUnderTest =
    GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Exceptions.SatelliteCandidateNotFoundException;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Exceptions.SatelliteCandidateNotFoundException;

/// <summary>
/// Covers the constructor of <c>SatelliteCandidateNotFoundException</c>.
/// </summary>
public sealed class ConstructorTests
{
    [Fact]
    public void KeepsTheCandidateName()
    {
        SatelliteCandidateNotFoundExceptionUnderTest exception = new("garage");

        Assert.Equal("garage", exception.CandidateName);
    }

    [Fact]
    public void NamesTheCandidateInTheMessage()
    {
        SatelliteCandidateNotFoundExceptionUnderTest exception = new("garage");

        Assert.Equal("No unknown satellite named 'garage' pinged recently.", exception.Message);
    }
}
