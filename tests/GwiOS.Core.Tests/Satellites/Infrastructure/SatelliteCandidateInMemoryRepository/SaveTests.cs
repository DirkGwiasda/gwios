using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;
using SatelliteCandidateInMemoryRepositoryUnderTest =
    GwiOS.Core.Satellites.Infrastructure.SatelliteCandidateInMemoryRepository;

namespace GwiOS.Core.Tests.Satellites.Infrastructure.SatelliteCandidateInMemoryRepository;

/// <summary>
/// Covers <c>SatelliteCandidateInMemoryRepository.Save</c>, <c>GetByName</c>, <c>GetToken</c> and <c>GetAll</c>, which
/// only make sense together.
/// </summary>
public sealed class SaveTests
{
    private readonly TestCandidates _candidates = new();
    private readonly SatelliteCandidateInMemoryRepositoryUnderTest _repository =
        new(new LoggerFake<SatelliteCandidateInMemoryRepositoryUnderTest>());

    [Fact]
    public void KeepsTheCandidateTogetherWithItsToken()
    {
        SatelliteCandidate candidate = _candidates.Create("garage");

        _repository.Save(candidate, TestCandidates.Token);

        Assert.Same(candidate, _repository.GetByName("garage"));
        Assert.Equal(TestCandidates.Token, _repository.GetToken("garage"));
        Assert.Same(candidate, Assert.Single(_repository.GetAll()));
    }

    [Fact]
    public void ReplacesTheCandidateAndTheTokenOfTheSameName()
    {
        _repository.Save(_candidates.Create("garage"), TestCandidates.Token);
        SatelliteCandidate laterCandidate = _candidates.Create("garage");

        _repository.Save(laterCandidate, TestCandidates.OtherToken);

        Assert.Same(laterCandidate, Assert.Single(_repository.GetAll()));
        Assert.Equal(TestCandidates.OtherToken, _repository.GetToken("garage"));
    }

    [Fact]
    public void ReturnsNull_ForAnUnknownName()
    {
        Assert.Null(_repository.GetByName("garage"));
        Assert.Null(_repository.GetToken("garage"));
        Assert.Empty(_repository.GetAll());
    }
}
