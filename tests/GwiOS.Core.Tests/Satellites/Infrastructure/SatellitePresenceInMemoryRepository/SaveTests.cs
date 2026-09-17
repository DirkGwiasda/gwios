using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;
using SatellitePresenceInMemoryRepositoryUnderTest =
    GwiOS.Core.Satellites.Infrastructure.SatellitePresenceInMemoryRepository;

namespace GwiOS.Core.Tests.Satellites.Infrastructure.SatellitePresenceInMemoryRepository;

/// <summary>
/// Covers <c>SatellitePresenceInMemoryRepository.Save</c>.
/// </summary>
public sealed class SaveTests
{
    private readonly LoggerFake<SatellitePresenceInMemoryRepositoryUnderTest> _logger = new();
    private readonly TestPresences _presences = new();
    private readonly SatellitePresenceInMemoryRepositoryUnderTest _repository;

    public SaveTests()
    {
        _repository = new SatellitePresenceInMemoryRepositoryUnderTest(_logger);
    }

    [Fact]
    public void ReplacesThePresenceOfTheSameSatellite()
    {
        _repository.Save(_presences.Create("keller"));
        SatellitePresence laterPresence = _presences.Create("keller");

        _repository.Save(laterPresence);

        Assert.Same(laterPresence, _repository.GetBySatelliteName("keller"));
    }

    [Fact]
    public void LogsOnlyTheFirstPresenceOfASatellite()
    {
        _repository.Save(_presences.Create("keller"));
        _repository.Save(_presences.Create("keller"));

        Assert.Equal("keller", Assert.Single(_logger.Entries).ContextData["SatelliteName"]);
    }
}
