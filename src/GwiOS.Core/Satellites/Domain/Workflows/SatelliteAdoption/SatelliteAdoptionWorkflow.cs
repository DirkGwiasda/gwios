using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteAdoption.Contracts;

namespace GwiOS.Core.Satellites.Domain.Workflows.SatelliteAdoption;

internal sealed class SatelliteAdoptionWorkflow(
    ISatelliteCandidateManager satelliteCandidateManager,
    ISatelliteManager satelliteManager,
    ILogger<SatelliteAdoptionWorkflow> logger) : ISatelliteAdoptionWorkflow
{
    private readonly ISatelliteCandidateManager _satelliteCandidateManager = satelliteCandidateManager;
    private readonly ISatelliteManager _satelliteManager = satelliteManager;
    private readonly ILogger<SatelliteAdoptionWorkflow> _logger = logger;

    public async Task AdoptCandidateAsync(
        string candidateName,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        string token = _satelliteCandidateManager.GetTokenForAdoption(candidateName);
        await _satelliteManager.CreateSatelliteWithTokenAsync(candidateName, displayName, token, cancellationToken);
        _satelliteCandidateManager.RemoveCandidate(candidateName);
        _logger.LogInformation("Unknown satellite adopted.", new Dictionary<string, string> { ["SatelliteName"] = candidateName });
    }
}
