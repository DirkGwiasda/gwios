using SatelliteCandidateTokenChangedExceptionUnderTest =
    GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Exceptions.SatelliteCandidateTokenChangedException;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Exceptions.SatelliteCandidateTokenChangedException;

/// <summary>
/// Covers the constructor of <c>SatelliteCandidateTokenChangedException</c>.
/// </summary>
public sealed class ConstructorTests
{
    [Fact]
    public void KeepsTheCandidateName()
    {
        SatelliteCandidateTokenChangedExceptionUnderTest exception = new("garage");

        Assert.Equal("garage", exception.CandidateName);
    }

    [Fact]
    public void NamesTheCandidateInTheMessage()
    {
        SatelliteCandidateTokenChangedExceptionUnderTest exception = new("garage");

        Assert.Equal("The unknown satellite 'garage' pinged with different tokens and cannot be adopted.", exception.Message);
    }
}
