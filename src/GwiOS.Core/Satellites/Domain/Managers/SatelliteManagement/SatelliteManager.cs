using System.Security.Cryptography;
using System.Text;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement;

internal sealed class SatelliteManager(
    ISatelliteRepository satelliteRepository,
    ISatelliteValidator satelliteValidator,
    ISatelliteTokenGenerator satelliteTokenGenerator,
    ISatelliteTokenProtector satelliteTokenProtector,
    TimeProvider timeProvider,
    ILogger<SatelliteManager> logger) : ISatelliteManager
{
    private readonly ISatelliteRepository _satelliteRepository = satelliteRepository;
    private readonly ISatelliteValidator _satelliteValidator = satelliteValidator;
    private readonly ISatelliteTokenGenerator _satelliteTokenGenerator = satelliteTokenGenerator;
    private readonly ISatelliteTokenProtector _satelliteTokenProtector = satelliteTokenProtector;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<SatelliteManager> _logger = logger;

    public async Task<string> CreateSatelliteAsync(
        string satelliteName,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        string token = _satelliteTokenGenerator.GenerateToken();
        await InsertSatelliteAsync(CreateSatellite(satelliteName, displayName, token), cancellationToken);
        return token;
    }

    public async Task CreateSatelliteWithTokenAsync(
        string satelliteName,
        string? displayName,
        string token,
        CancellationToken cancellationToken = default)
    {
        ThrowIfInvalidToken(satelliteName, token);
        await InsertSatelliteAsync(CreateSatellite(satelliteName, displayName, token), cancellationToken);
    }

    public async Task CreateLockedSatelliteAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        Satellite satellite = CreateSatellite(satelliteName, displayName: null, token: null);
        satellite.IsLocked = true;
        await InsertSatelliteAsync(satellite, cancellationToken);
    }

    public async Task ChangeDisplayNameAsync(
        string satelliteName,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        Satellite satellite = await GetStoredSatelliteAsync(satelliteName, cancellationToken);
        satellite.DisplayName = NormalizeDisplayName(displayName);
        ThrowIfInvalid(satellite);
        await UpdateSatelliteAsync(satellite, "Satellite display name changed.", cancellationToken);
    }

    public async Task LockSatelliteAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        Satellite satellite = await GetStoredSatelliteAsync(satelliteName, cancellationToken);
        satellite.IsLocked = true;
        await UpdateSatelliteAsync(satellite, "Satellite locked.", cancellationToken);
    }

    public async Task UnlockSatelliteAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        Satellite satellite = await GetStoredSatelliteAsync(satelliteName, cancellationToken);
        satellite.IsLocked = false;
        await UpdateSatelliteAsync(satellite, "Satellite unlocked.", cancellationToken);
    }

    public async Task<string> RenewTokenAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        Satellite satellite = await GetStoredSatelliteAsync(satelliteName, cancellationToken);
        string token = _satelliteTokenGenerator.GenerateToken();
        satellite.ProtectedToken = _satelliteTokenProtector.Protect(token);
        await UpdateSatelliteAsync(satellite, "Satellite token renewed.", cancellationToken);
        return token;
    }

    public async Task<List<Satellite>> GetAllSatellitesAsync(CancellationToken cancellationToken = default)
        => await _satelliteRepository.GetAllAsync(cancellationToken);

    public async Task DeleteSatelliteAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        await _satelliteRepository.DeleteAsync(satelliteName, cancellationToken);
        _logger.LogInformation("Satellite deleted.", CreateSatelliteNameData(satelliteName));
    }

    public async Task VerifyTokenAsync(
        string satelliteName,
        string token,
        CancellationToken cancellationToken = default)
    {
        string storedToken = await GetTokenAsync(satelliteName, cancellationToken);
        if (!AreEqualInConstantTime(storedToken, token))
        {
            _logger.LogWarning("Satellite token is wrong.", CreateSatelliteNameData(satelliteName));
            throw new SatelliteTokenRejectedException(satelliteName);
        }
    }

    public async Task<string> GetTokenAsync(string satelliteName, CancellationToken cancellationToken = default)
    {
        Satellite satellite = await GetStoredSatelliteAsync(satelliteName, cancellationToken);
        if (satellite.IsLocked || (satellite.ProtectedToken is null))
        {
            _logger.LogWarning("Satellite is locked or has no token.", CreateSatelliteNameData(satelliteName));
            throw new SatelliteTokenRejectedException(satelliteName);
        }

        if (!_satelliteTokenProtector.TryUnprotect(satellite.ProtectedToken, out string token))
        {
            throw new SatelliteTokenRejectedException(satelliteName);
        }

        return token;
    }

    private Satellite CreateSatellite(string satelliteName, string? displayName, string? token)
        => new()
        {
            Name = satelliteName,
            DisplayName = NormalizeDisplayName(displayName),
            ProtectedToken = token is null ? null : _satelliteTokenProtector.Protect(token),
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
        };

    private async Task InsertSatelliteAsync(Satellite satellite, CancellationToken cancellationToken)
    {
        ThrowIfInvalid(satellite);
        await _satelliteRepository.InsertAsync(satellite, cancellationToken);
        _logger.LogInformation("Satellite created.", CreateSatelliteNameData(satellite.Name));
    }

    private async Task UpdateSatelliteAsync(Satellite satellite, string logMessage, CancellationToken cancellationToken)
    {
        await _satelliteRepository.UpdateAsync(satellite, cancellationToken);
        _logger.LogInformation(logMessage, CreateSatelliteNameData(satellite.Name));
    }

    private async Task<Satellite> GetStoredSatelliteAsync(string satelliteName, CancellationToken cancellationToken)
        => await _satelliteRepository.GetByNameAsync(satelliteName, cancellationToken)
            ?? throw new SatelliteNotFoundException(satelliteName);

    private void ThrowIfInvalid(Satellite satellite)
    {
        if (!_satelliteValidator.IsValid(satellite, out List<string> validationErrors))
        {
            throw new SatelliteValidationException(satellite.Name, validationErrors);
        }
    }

    private void ThrowIfInvalidToken(string satelliteName, string token)
    {
        if (!_satelliteValidator.IsValidToken(token))
        {
            throw new SatelliteValidationException(
                satelliteName,
                ["The token must consist of 32 to 128 characters out of the base64url alphabet."]);
        }
    }

    private static string? NormalizeDisplayName(string? displayName)
        => string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();

    // A comparison that stops at the first difference would reveal through its duration how much of a guess is right.
    private static bool AreEqualInConstantTime(string expected, string actual)
        => CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(actual));

    private static Dictionary<string, string> CreateSatelliteNameData(string satelliteName)
        => new() { ["SatelliteName"] = satelliteName };
}
