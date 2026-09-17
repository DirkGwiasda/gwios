using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;

/// <summary>
/// Stands in for <see cref="ISatellitePresenceValidator"/>. It considers every presence valid until a test sets
/// <see cref="ValidationErrors"/>.
/// </summary>
public sealed class SatellitePresenceValidatorFake : ISatellitePresenceValidator
{
    /// <summary>
    /// The validation errors reported for every presence. Empty by default, which makes every presence valid.
    /// </summary>
    public List<string> ValidationErrors { get; set; } = [];

    public bool IsValid(SatellitePresence presence, out List<string> validationErrors)
    {
        validationErrors = [.. ValidationErrors];
        return validationErrors.Count == 0;
    }
}
