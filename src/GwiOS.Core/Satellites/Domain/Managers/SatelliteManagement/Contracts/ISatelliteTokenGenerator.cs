namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;

/// <summary>
/// Creates new random satellite tokens.
/// </summary>
public interface ISatelliteTokenGenerator
{
    /// <summary>
    /// Returns a new token from a cryptographically secure random source, in the format satellites accept.
    /// </summary>
    string GenerateToken();
}
