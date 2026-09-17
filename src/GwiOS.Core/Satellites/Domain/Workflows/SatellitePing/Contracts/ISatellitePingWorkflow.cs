using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Workflows.SatellitePing.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Workflows.SatellitePing.Contracts;

/// <summary>
/// Processes the pings satellites send to GwiOS.
/// </summary>
public interface ISatellitePingWorkflow
{
    /// <summary>
    /// Checks the token of the given ping and records what the satellite reported. Returns normally if the ping is
    /// accepted. Throws a <see cref="SatelliteNotFoundException"/> if no satellite has the name of the ping, after
    /// recording the ping as an unknown satellite; a <see cref="SatelliteTokenRejectedException"/> if the token is
    /// rejected; and a <see cref="SatellitePresenceValidationException"/> if the reported values are invalid.
    /// </summary>
    Task ProcessPingAsync(SatellitePingData ping, CancellationToken cancellationToken = default);
}
