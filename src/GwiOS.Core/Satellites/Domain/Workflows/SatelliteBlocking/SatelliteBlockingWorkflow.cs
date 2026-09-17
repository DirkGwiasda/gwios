using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteBlocking.Contracts;

namespace GwiOS.Core.Satellites.Domain.Workflows.SatelliteBlocking;

internal sealed class SatelliteBlockingWorkflow(
    ISatelliteManager satelliteManager,
    ISatelliteCandidateManager satelliteCandidateManager,
    ILogger<SatelliteBlockingWorkflow> logger) : ISatelliteBlockingWorkflow
{
    private readonly ISatelliteManager _satelliteManager = satelliteManager;
    private readonly ISatelliteCandidateManager _satelliteCandidateManager = satelliteCandidateManager;
    private readonly ILogger<SatelliteBlockingWorkflow> _logger = logger;

    public async Task BlockCandidateAsync(string candidateName, CancellationToken cancellationToken = default)
    {
        await _satelliteManager.CreateLockedSatelliteAsync(candidateName, cancellationToken);
        _satelliteCandidateManager.RemoveCandidate(candidateName);
        _logger.LogInformation("Unknown satellite blocked.", new Dictionary<string, string> { ["SatelliteName"] = candidateName });
    }
}
