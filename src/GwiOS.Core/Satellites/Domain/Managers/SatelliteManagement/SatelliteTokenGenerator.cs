using System.Buffers.Text;
using System.Security.Cryptography;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;

namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement;

internal sealed class SatelliteTokenGenerator(ILogger<SatelliteTokenGenerator> logger) : ISatelliteTokenGenerator
{
    // 32 random bytes give 256 bits of entropy and 43 base64url characters, within the 32 to 128 satellites accept.
    private const int TokenByteCount = 32;

    private readonly ILogger<SatelliteTokenGenerator> _logger = logger;

    public string GenerateToken()
    {
        string token = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(TokenByteCount));
        _logger.LogDebug("Satellite token generated.");
        return token;
    }
}
