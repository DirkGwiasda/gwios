namespace GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when a measurement reading of a satellite violates at least one validation rule.
/// </summary>
/// <param name="satelliteName">The name of the satellite the reading came from.</param>
/// <param name="validationErrors">One message per violated rule; never empty.</param>
public sealed class MeasurementValidationException(string satelliteName, IReadOnlyList<string> validationErrors)
    : Exception($"The measurements of satellite '{satelliteName}' are invalid: {string.Join(" ", validationErrors)}")
{
    /// <summary>
    /// The name of the satellite the reading came from.
    /// </summary>
    public string SatelliteName { get; } = satelliteName;

    /// <summary>
    /// One message per violated rule; never empty.
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; } = validationErrors;
}
