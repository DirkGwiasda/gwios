using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;
using SatelliteCandidateInMemoryRepositoryUnderTest =
    GwiOS.Core.Satellites.Infrastructure.SatelliteCandidateInMemoryRepository;

namespace GwiOS.Core.Tests.Satellites.Infrastructure.SatelliteCandidateInMemoryRepository;

/// <summary>
/// Covers <c>SatelliteCandidateInMemoryRepository.Remove</c>.
/// </summary>
public sealed class RemoveTests
{
    private readonly LoggerFake<SatelliteCandidateInMemoryRepositoryUnderTest> _logger = new();
    private readonly TestCandidates _candidates = new();
    private readonly SatelliteCandidateInMemoryRepositoryUnderTest _repository;

    public RemoveTests()
    {
        _repository = new SatelliteCandidateInMemoryRepositoryUnderTest(_logger);
    }

    [Fact]
    public void RemovesTheCandidateAndItsToken()
    {
        _repository.Save(_candidates.Create("garage"), TestCandidates.Token);
        _repository.Save(_candidates.Create("keller"), TestCandidates.Token);

        _repository.Remove("garage");

        Assert.Null(_repository.GetByName("garage"));
        Assert.Null(_repository.GetToken("garage"));
        Assert.Equal("keller", Assert.Single(_repository.GetAll()).Name);
        Assert.Single(_logger.Entries);
    }

    [Fact]
    public void DoesNothingAndLogsNothing_ForAnUnknownName()
    {
        _repository.Remove("garage");

        Assert.Empty(_logger.Entries);
    }
}
