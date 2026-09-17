namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when an operation requires a stored satellite, but no satellite with the given name is stored.
/// </summary>
/// <param name="satelliteName">The name that no stored satellite has.</param>
public sealed class SatelliteNotFoundException(string satelliteName)
    : Exception($"No satellite named '{satelliteName}' is stored.")
{
    /// <summary>
    /// The name that no stored satellite has.
    /// </summary>
    public string SatelliteName { get; } = satelliteName;
}
