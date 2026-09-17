namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when a satellite or its token is rejected because it violates at least one validation rule.
/// </summary>
/// <param name="satelliteName">The name of the rejected satellite, as given.</param>
/// <param name="validationErrors">One message per violated rule; never empty.</param>
public sealed class SatelliteValidationException(string satelliteName, IReadOnlyList<string> validationErrors)
    : Exception($"Satellite '{satelliteName}' is invalid: {string.Join(" ", validationErrors)}")
{
    /// <summary>
    /// The name of the rejected satellite, as given.
    /// </summary>
    public string SatelliteName { get; } = satelliteName;

    /// <summary>
    /// One message per violated rule; never empty.
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; } = validationErrors;
}
