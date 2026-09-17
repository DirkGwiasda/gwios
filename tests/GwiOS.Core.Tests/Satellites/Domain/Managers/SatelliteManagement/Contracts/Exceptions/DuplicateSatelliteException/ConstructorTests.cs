using DuplicateSatelliteExceptionUnderTest =
    GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions.DuplicateSatelliteException;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions.DuplicateSatelliteException;

/// <summary>
/// Covers the constructor of <c>DuplicateSatelliteException</c>.
/// </summary>
public sealed class ConstructorTests
{
    private readonly InvalidOperationException _innerException = new("Unique violation.");

    [Fact]
    public void KeepsTheSatelliteNameAndTheInnerException()
    {
        DuplicateSatelliteExceptionUnderTest exception = new("keller", _innerException);

        Assert.Equal("keller", exception.SatelliteName);
        Assert.Same(_innerException, exception.InnerException);
    }

    [Fact]
    public void NamesTheSatelliteInTheMessage()
    {
        DuplicateSatelliteExceptionUnderTest exception = new("keller", _innerException);

        Assert.Equal("Satellite 'keller' cannot be stored because the name is already used.", exception.Message);
    }
}
