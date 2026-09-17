using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;

namespace GwiOS.WebUI.Tests.TestInfrastructure;

/// <summary>
/// Stands in for <see cref="ISatelliteManager"/> by keeping satellites in memory, ordered by name. Like the real
/// manager it rejects names that are already used and changes of satellites that are not stored, and it rejects names
/// outside <c>a–z</c>, <c>0–9</c> and <c>-</c>. New tokens are <c>token-1</c>, <c>token-2</c> and so on.
/// </summary>
public sealed class SatelliteManagerFake : ISatelliteManager
{
    private readonly List<Satellite> _satellites = [];
    private int _tokenCount;

    /// <summary>
    /// The stored satellites, ordered by name.
    /// </summary>
    public IReadOnlyList<Satellite> Satellites
        => [.. _satellites.OrderBy(satellite => satellite.Name, StringComparer.Ordinal)];

    /// <summary>
    /// The display names passed to the latest create call; empty until one was made.
    /// </summary>
    public List<string?> CreatedDisplayNames { get; } = [];

    /// <summary>
    /// Stores the given satellite directly, as if it had been created before the test.
    /// </summary>
    public void Add(Satellite satellite)
        => _satellites.Add(satellite);

    /// <summary>
    /// Deletes the satellite with the given name directly, as if someone else had deleted it.
    /// </summary>
    public void Remove(string name)
        => _satellites.RemoveAll(satellite => satellite.Name == name);

    public Task<string> CreateSatelliteAsync(string satelliteName, string? displayName, CancellationToken cancellationToken = default)
    {
        if (!satelliteName.All(character => char.IsAsciiLetterLower(character) || char.IsAsciiDigit(character) || character == '-'))
        {
            throw new SatelliteValidationException(satelliteName, ["Invalid name."]);
        }

        ThrowIfDuplicate(satelliteName);
        CreatedDisplayNames.Add(displayName);
        _satellites.Add(new Satellite { Name = satelliteName, DisplayName = Normalize(displayName), ProtectedToken = "protected" });
        return Task.FromResult($"token-{++_tokenCount}");
    }

    public Task CreateSatelliteWithTokenAsync(string satelliteName, string? displayName, string token, CancellationToken cancellationToken = default)
    {
        ThrowIfDuplicate(satelliteName);
        _satellites.Add(new Satellite { Name = satelliteName, DisplayName = Normalize(displayName), ProtectedToken = "protected" });
        return Task.CompletedTask;
    }

    public Task CreateLockedSatelliteAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        ThrowIfDuplicate(satelliteName);
        _satellites.Add(new Satellite { Name = satelliteName, IsLocked = true });
        return Task.CompletedTask;
    }

    public Task ChangeDisplayNameAsync(string satelliteName, string? displayName, CancellationToken cancellationToken = default)
    {
        Replace(satelliteName, satellite => satellite.DisplayName = Normalize(displayName));
        return Task.CompletedTask;
    }

    public Task LockSatelliteAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        Replace(satelliteName, satellite => satellite.IsLocked = true);
        return Task.CompletedTask;
    }

    public Task UnlockSatelliteAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        Replace(satelliteName, satellite => satellite.IsLocked = false);
        return Task.CompletedTask;
    }

    public Task<string> RenewTokenAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        Replace(satelliteName, satellite => satellite.ProtectedToken = "renewed");
        return Task.FromResult($"token-{++_tokenCount}");
    }

    public Task<List<Satellite>> GetAllSatellitesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Satellites.ToList());

    public Task DeleteSatelliteAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        Remove(satelliteName);
        return Task.CompletedTask;
    }

    public Task VerifyTokenAsync(string satelliteName, string token, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("The pages do not verify tokens.");

    public Task<string> GetTokenAsync(string satelliteName, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("The pages do not read tokens.");

    // Stored satellites are replaced by changed copies, so a page cannot see a change before it reloads.
    private void Replace(string satelliteName, Action<Satellite> change)
    {
        int index = _satellites.FindIndex(satellite => satellite.Name == satelliteName);
        if (index < 0)
        {
            throw new SatelliteNotFoundException(satelliteName);
        }

        Satellite stored = _satellites[index];
        Satellite copy = new()
        {
            Name = stored.Name,
            DisplayName = stored.DisplayName,
            IsLocked = stored.IsLocked,
            ProtectedToken = stored.ProtectedToken,
            CreatedAt = stored.CreatedAt
        };
        change(copy);
        _satellites[index] = copy;
    }

    private void ThrowIfDuplicate(string satelliteName)
    {
        if (_satellites.Any(satellite => satellite.Name == satelliteName))
        {
            throw new DuplicateSatelliteException(satelliteName, new InvalidOperationException("Name already used."));
        }
    }

    private static string? Normalize(string? displayName)
        => string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
}
