using System.Security.Cryptography;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using Microsoft.AspNetCore.DataProtection;

namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement;

internal sealed class SatelliteTokenProtector(
    IDataProtectionProvider dataProtectionProvider,
    ILogger<SatelliteTokenProtector> logger) : ISatelliteTokenProtector
{
    // Changing the purpose makes all stored tokens unreadable.
    private const string Purpose = "GwiOS.Satellites.Token";

    private readonly IDataProtector _dataProtector = dataProtectionProvider.CreateProtector(Purpose);
    private readonly ILogger<SatelliteTokenProtector> _logger = logger;

    public string Protect(string token)
        => _dataProtector.Protect(token);

    public bool TryUnprotect(string protectedToken, out string token)
    {
        try
        {
            token = _dataProtector.Unprotect(protectedToken);
            return true;
        }
        catch (CryptographicException exception)
        {
            _logger.LogError("Satellite token cannot be decrypted.", CreateExceptionData(exception));
            token = string.Empty;
            return false;
        }
    }

    // The message of the exception is logged, never the token.
    private static Dictionary<string, string> CreateExceptionData(CryptographicException exception)
        => new() { ["Reason"] = exception.Message };
}
