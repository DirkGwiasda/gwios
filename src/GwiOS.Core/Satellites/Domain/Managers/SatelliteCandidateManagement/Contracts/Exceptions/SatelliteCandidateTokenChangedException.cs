namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when a satellite candidate cannot be adopted because it pinged with different tokens.
/// </summary>
/// <param name="candidateName">The name of the candidate.</param>
public sealed class SatelliteCandidateTokenChangedException(string candidateName)
    : Exception($"The unknown satellite '{candidateName}' pinged with different tokens and cannot be adopted.")
{
    /// <summary>
    /// The name of the candidate.
    /// </summary>
    public string CandidateName { get; } = candidateName;
}
