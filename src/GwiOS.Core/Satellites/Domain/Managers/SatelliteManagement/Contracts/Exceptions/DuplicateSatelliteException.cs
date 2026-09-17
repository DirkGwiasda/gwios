namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when a satellite cannot be stored because another satellite already has its name.
/// </summary>
/// <param name="satelliteName">The name that is already used.</param>
/// <param name="innerException">The storage error that revealed the conflict.</param>
public sealed class DuplicateSatelliteException(string satelliteName, Exception innerException)
    : Exception($"Satellite '{satelliteName}' cannot be stored because the name is already used.", innerException)
{
    /// <summary>
    /// The name that is already used.
    /// </summary>
    public string SatelliteName { get; } = satelliteName;
}
