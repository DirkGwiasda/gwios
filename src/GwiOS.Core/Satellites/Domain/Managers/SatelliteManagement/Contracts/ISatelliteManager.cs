using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;

/// <summary>
/// Manages the satellites of GwiOS: creates, changes, locks, reads and deletes them, and checks and hands out their
/// tokens. Methods that take a display name treat an empty one as none and trim it.
/// </summary>
public interface ISatelliteManager
{
    /// <summary>
    /// Creates a satellite with a new random token and returns that token in plain text; it cannot be read again
    /// later. Throws a <see cref="SatelliteValidationException"/> if name or display name are invalid and a
    /// <see cref="DuplicateSatelliteException"/> if the name is already used.
    /// </summary>
    Task<string> CreateSatelliteAsync(
        string satelliteName,
        string? displayName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a satellite with the given token, such as the token an unknown satellite pinged with. Throws a
    /// <see cref="SatelliteValidationException"/> if name, display name or token are invalid and a
    /// <see cref="DuplicateSatelliteException"/> if the name is already used.
    /// </summary>
    Task CreateSatelliteWithTokenAsync(
        string satelliteName,
        string? displayName,
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a locked satellite without token, which blocks all pings with its name. Throws a
    /// <see cref="SatelliteValidationException"/> if the name is invalid and a
    /// <see cref="DuplicateSatelliteException"/> if it is already used.
    /// </summary>
    Task CreateLockedSatelliteAsync(string satelliteName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the display name of the satellite with the given name. Throws a
    /// <see cref="SatelliteValidationException"/> if the display name is invalid and a
    /// <see cref="SatelliteNotFoundException"/> if no such satellite is stored.
    /// </summary>
    Task ChangeDisplayNameAsync(
        string satelliteName,
        string? displayName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Locks the satellite with the given name. Throws a <see cref="SatelliteNotFoundException"/> if no such
    /// satellite is stored.
    /// </summary>
    Task LockSatelliteAsync(string satelliteName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unlocks the satellite with the given name. A satellite without token stays unusable until it gets a new token.
    /// Throws a <see cref="SatelliteNotFoundException"/> if no such satellite is stored.
    /// </summary>
    Task UnlockSatelliteAsync(string satelliteName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the token of the satellite with the given name by a new random token and returns it in plain text;
    /// the old token stops working. Throws a <see cref="SatelliteNotFoundException"/> if no such satellite is stored.
    /// </summary>
    Task<string> RenewTokenAsync(string satelliteName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all stored satellites, ordered by name. The list is empty if no satellites are stored.
    /// </summary>
    Task<List<Satellite>> GetAllSatellitesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the satellite with the given name. Does nothing if no such satellite is stored.
    /// </summary>
    Task DeleteSatelliteAsync(string satelliteName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks the given token against the satellite with the given name, in constant time. Returns normally if the
    /// token is accepted. Throws a <see cref="SatelliteNotFoundException"/> if no such satellite is stored and a
    /// <see cref="SatelliteTokenRejectedException"/> if the token is wrong or the satellite is locked, has no token
    /// or its token cannot be decrypted.
    /// </summary>
    Task VerifyTokenAsync(string satelliteName, string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the plain token of the satellite with the given name, which GwiOS sends when it fetches measurements.
    /// Throws a <see cref="SatelliteNotFoundException"/> if no such satellite is stored and a
    /// <see cref="SatelliteTokenRejectedException"/> if the satellite is locked, has no token or its token cannot be
    /// decrypted.
    /// </summary>
    Task<string> GetTokenAsync(string satelliteName, CancellationToken cancellationToken = default);
}
