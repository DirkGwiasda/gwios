using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.Contracts;

/// <summary>
/// Stands in for <see cref="ISatelliteTokenProtector"/> with a visible "encryption": it puts <see cref="Prefix"/> in
/// front of the token. Decrypting fails for values without the prefix and for all values once a test clears
/// <see cref="CanUnprotect"/>.
/// </summary>
public sealed class SatelliteTokenProtectorFake : ISatelliteTokenProtector
{
    /// <summary>
    /// The text put in front of a token to "encrypt" it.
    /// </summary>
    public const string Prefix = "protected:";

    /// <summary>
    /// Whether tokens can be decrypted, as if the keys were available. <c>true</c> by default.
    /// </summary>
    public bool CanUnprotect { get; set; } = true;

    public string Protect(string token)
        => Prefix + token;

    public bool TryUnprotect(string protectedToken, out string token)
    {
        if (CanUnprotect && protectedToken.StartsWith(Prefix, StringComparison.Ordinal))
        {
            token = protectedToken[Prefix.Length..];
            return true;
        }

        token = string.Empty;
        return false;
    }
}
