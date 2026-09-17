using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.Contracts;

/// <summary>
/// Stands in for <see cref="ISatelliteManager"/> for the workflow tests. It keeps satellites with their plain tokens in
/// memory and checks tokens like the real manager, but does not validate.
/// </summary>
public sealed class SatelliteManagerFake : ISatelliteManager
{
    private readonly List<Satellite> _satellites = [];
    private readonly Dictionary<string, string> _tokens = [];

    /// <summary>
    /// The stored satellites, ordered by name.
    /// </summary>
    public IReadOnlyList<Satellite> Satellites
        => [.. _satellites.OrderBy(satellite => satellite.Name, StringComparer.Ordinal)];

    /// <summary>
    /// The plain tokens of the stored satellites by name.
    /// </summary>
    public IReadOnlyDictionary<string, string> Tokens
        => _tokens;

    /// <summary>
    /// Stores a satellite with the given name and token directly, as if it had been created before the test.
    /// </summary>
    public void Add(string name, string? token = "stored-token", bool isLocked = false)
    {
        _satellites.Add(new Satellite { Name = name, IsLocked = isLocked, ProtectedToken = token is null ? null : "protected" });
        if (token is not null)
        {
            _tokens[name] = token;
        }
    }

    public Task<string> CreateSatelliteAsync(string satelliteName, string? displayName, CancellationToken cancellationToken = default)
    {
        Store(satelliteName, displayName, "new-token", isLocked: false);
        return Task.FromResult("new-token");
    }

    public Task CreateSatelliteWithTokenAsync(
        string satelliteName,
        string? displayName,
        string token,
        CancellationToken cancellationToken = default)
    {
        Store(satelliteName, displayName, token, isLocked: false);
        return Task.CompletedTask;
    }

    public Task CreateLockedSatelliteAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        Store(satelliteName, displayName: null, token: null, isLocked: true);
        return Task.CompletedTask;
    }

    public Task ChangeDisplayNameAsync(string satelliteName, string? displayName, CancellationToken cancellationToken = default)
    {
        GetSatellite(satelliteName).DisplayName = displayName;
        return Task.CompletedTask;
    }

    public Task LockSatelliteAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        GetSatellite(satelliteName).IsLocked = true;
        return Task.CompletedTask;
    }

    public Task UnlockSatelliteAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        GetSatellite(satelliteName).IsLocked = false;
        return Task.CompletedTask;
    }

    public Task<string> RenewTokenAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        GetSatellite(satelliteName);
        _tokens[satelliteName] = "renewed-token";
        return Task.FromResult("renewed-token");
    }

    public Task<List<Satellite>> GetAllSatellitesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Satellites.ToList());

    public Task DeleteSatelliteAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        _satellites.RemoveAll(satellite => satellite.Name == satelliteName);
        _tokens.Remove(satelliteName);
        return Task.CompletedTask;
    }

    public async Task VerifyTokenAsync(string satelliteName, string token, CancellationToken cancellationToken = default)
    {
        if (await GetTokenAsync(satelliteName, cancellationToken) != token)
        {
            throw new SatelliteTokenRejectedException(satelliteName);
        }
    }

    public Task<string> GetTokenAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        Satellite satellite = GetSatellite(satelliteName);
        return satellite.IsLocked || !_tokens.TryGetValue(satelliteName, out string? token)
            ? throw new SatelliteTokenRejectedException(satelliteName)
            : Task.FromResult(token);
    }

    private void Store(string satelliteName, string? displayName, string? token, bool isLocked)
    {
        if (_satellites.Any(satellite => satellite.Name == satelliteName))
        {
            throw new DuplicateSatelliteException(satelliteName, new InvalidOperationException("Name already used."));
        }

        Add(satelliteName, token, isLocked);
        GetSatellite(satelliteName).DisplayName = displayName;
    }

    private Satellite GetSatellite(string satelliteName)
        => _satellites.SingleOrDefault(satellite => satellite.Name == satelliteName)
            ?? throw new SatelliteNotFoundException(satelliteName);
}
