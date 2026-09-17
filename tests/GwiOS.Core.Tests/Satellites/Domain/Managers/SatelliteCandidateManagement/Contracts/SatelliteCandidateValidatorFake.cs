using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;

/// <summary>
/// Stands in for <see cref="ISatelliteCandidateValidator"/>. Every candidate is valid until a test clears
/// <see cref="IsCandidateValid"/>.
/// </summary>
public sealed class SatelliteCandidateValidatorFake : ISatelliteCandidateValidator
{
    /// <summary>
    /// Whether every candidate is valid. <c>true</c> by default.
    /// </summary>
    public bool IsCandidateValid { get; set; } = true;

    public bool IsValid(SatelliteCandidate candidate, string token)
        => IsCandidateValid;
}
