using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;

namespace GwiOS.Core.Satellites.Domain.Workflows.SatelliteAdoption.Contracts;

/// <summary>
/// Registers unknown satellites with the token they pinged with.
/// </summary>
public interface ISatelliteAdoptionWorkflow
{
    /// <summary>
    /// Creates a satellite from the candidate with the given name, using the token it pinged with and the given
    /// display name, and removes the candidate. Throws a <see cref="SatelliteCandidateNotFoundException"/> if no such
    /// candidate exists, a <see cref="SatelliteCandidateTokenChangedException"/> if it pinged with different tokens, a
    /// <see cref="SatelliteValidationException"/> if the display name is invalid and a
    /// <see cref="DuplicateSatelliteException"/> if a satellite with the name exists already.
    /// </summary>
    Task AdoptCandidateAsync(string candidateName, string? displayName, CancellationToken cancellationToken = default);
}
