namespace GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when the values a satellite reported with its ping violate at least one validation rule.
/// </summary>
/// <param name="satelliteName">The name of the satellite the ping came from.</param>
/// <param name="validationErrors">One message per violated rule; never empty.</param>
public sealed class SatellitePresenceValidationException(string satelliteName, IReadOnlyList<string> validationErrors)
    : Exception($"The ping of satellite '{satelliteName}' is invalid: {string.Join(" ", validationErrors)}")
{
    /// <summary>
    /// The name of the satellite the ping came from.
    /// </summary>
    public string SatelliteName { get; } = satelliteName;

    /// <summary>
    /// One message per violated rule; never empty.
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; } = validationErrors;
}
