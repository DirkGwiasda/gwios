namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when an operation requires a satellite candidate, but no current candidate has the given name.
/// </summary>
/// <param name="candidateName">The name that no current candidate has.</param>
public sealed class SatelliteCandidateNotFoundException(string candidateName)
    : Exception($"No unknown satellite named '{candidateName}' pinged recently.")
{
    /// <summary>
    /// The name that no current candidate has.
    /// </summary>
    public string CandidateName { get; } = candidateName;
}
