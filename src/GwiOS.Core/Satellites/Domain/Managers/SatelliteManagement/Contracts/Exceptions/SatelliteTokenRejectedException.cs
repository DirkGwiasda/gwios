namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when the token of a stored satellite cannot be accepted or used: the given token is wrong, or the satellite
/// is locked, has no token or its token cannot be decrypted.
/// </summary>
/// <param name="satelliteName">The name of the satellite whose token is rejected.</param>
public sealed class SatelliteTokenRejectedException(string satelliteName)
    : Exception($"The token of satellite '{satelliteName}' is rejected.")
{
    /// <summary>
    /// The name of the satellite whose token is rejected.
    /// </summary>
    public string SatelliteName { get; } = satelliteName;
}
