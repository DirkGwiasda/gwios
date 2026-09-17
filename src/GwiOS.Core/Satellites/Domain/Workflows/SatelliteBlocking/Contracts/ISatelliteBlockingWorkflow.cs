using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;

namespace GwiOS.Core.Satellites.Domain.Workflows.SatelliteBlocking.Contracts;

/// <summary>
/// Blocks unknown satellites.
/// </summary>
public interface ISatelliteBlockingWorkflow
{
    /// <summary>
    /// Creates a locked satellite without token with the given name, so all further pings with it are rejected, and
    /// removes the candidate with that name if there is one. Throws a <see cref="SatelliteValidationException"/> if
    /// the name is invalid and a <see cref="DuplicateSatelliteException"/> if a satellite with the name exists
    /// already.
    /// </summary>
    Task BlockCandidateAsync(string candidateName, CancellationToken cancellationToken = default);
}
