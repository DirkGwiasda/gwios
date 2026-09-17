using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;

/// <summary>
/// Checks the values of a single satellite and the format of satellite tokens.
/// </summary>
public interface ISatelliteValidator
{
    /// <summary>
    /// Returns whether name and display name of the given satellite are valid. <paramref name="validationErrors"/>
    /// receives one message per violated rule; it is empty if the satellite is valid.
    /// </summary>
    bool IsValid(Satellite satellite, out List<string> validationErrors);

    /// <summary>
    /// Returns whether the given plain token has the format satellites accept: 32 to 128 characters out of the
    /// base64url alphabet.
    /// </summary>
    bool IsValidToken(string token);
}
