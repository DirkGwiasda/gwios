using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;
using SatellitePresenceInMemoryRepositoryUnderTest =
    GwiOS.Core.Satellites.Infrastructure.SatellitePresenceInMemoryRepository;

namespace GwiOS.Core.Tests.Satellites.Infrastructure.SatellitePresenceInMemoryRepository;

/// <summary>
/// Covers <c>SatellitePresenceInMemoryRepository.GetBySatelliteName</c>.
/// </summary>
public sealed class GetBySatelliteNameTests
{
    private readonly TestPresences _presences = new();
    private readonly SatellitePresenceInMemoryRepositoryUnderTest _repository =
        new(new LoggerFake<SatellitePresenceInMemoryRepositoryUnderTest>());

    [Fact]
    public void ReturnsThePresenceOfTheSatellite()
    {
        SatellitePresence presence = _presences.Create("keller");
        _repository.Save(presence);
        _repository.Save(_presences.Create("garage"));

        Assert.Same(presence, _repository.GetBySatelliteName("keller"));
    }

    [Fact]
    public void ReturnsNull_WhenNoPresenceOfTheSatelliteIsStored()
    {
        Assert.Null(_repository.GetBySatelliteName("keller"));
    }
}
