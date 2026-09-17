namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;

/// <summary>
/// Encrypts satellite tokens for storage and decrypts them again.
/// </summary>
public interface ISatelliteTokenProtector
{
    /// <summary>
    /// Returns the given plain token in encrypted form, suitable for storage.
    /// </summary>
    string Protect(string token);

    /// <summary>
    /// Decrypts the given encrypted token. Returns <c>false</c> and an empty <paramref name="token"/> if it cannot be
    /// decrypted, for example because the keys it was encrypted with are lost.
    /// </summary>
    bool TryUnprotect(string protectedToken, out string token);
}
