using System.Text.RegularExpressions;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement;

internal sealed partial class SatellitePresenceValidator(ILogger<SatellitePresenceValidator> logger)
    : ISatellitePresenceValidator
{
    private const int MaxTextLength = 64;
    private const int MinWifiRssiDbm = -127;
    private const int MaxWifiRssiDbm = 0;
    private const int MaxSensorCount = 8;
    private const int MaxQuantityCount = 16;

    private readonly ILogger<SatellitePresenceValidator> _logger = logger;

    public bool IsValid(SatellitePresence presence, out List<string> validationErrors)
    {
        validationErrors = CollectValidationErrors(presence);
        if (validationErrors.Count == 0)
        {
            return true;
        }

        LogValidationErrors(presence.SatelliteName, validationErrors);
        return false;
    }

    private static List<string> CollectValidationErrors(SatellitePresence presence)
    {
        List<string> validationErrors = [];
        if (!IsValidEndpoint(presence.Endpoint))
        {
            validationErrors.Add(
                "The endpoint must be an http address of at most 64 characters without path, query or user info.");
        }

        if (!IsValidText(presence.Board))
        {
            validationErrors.Add("The board must consist of 1 to 64 characters without control characters.");
        }

        if (!IsValidText(presence.Firmware))
        {
            validationErrors.Add("The firmware must consist of 1 to 64 characters without control characters.");
        }

        if ((presence.WifiRssiDbm < MinWifiRssiDbm) || (presence.WifiRssiDbm > MaxWifiRssiDbm))
        {
            validationErrors.Add($"The WLAN signal strength must lie between {MinWifiRssiDbm} and {MaxWifiRssiDbm} dBm.");
        }

        if (!AreValidSensors(presence.Sensors))
        {
            validationErrors.Add(
                $"At most {MaxSensorCount} sensors with 1 to {MaxQuantityCount} quantities each are allowed; types "
                + "and quantities must consist of 1 to 32 characters out of a-z, 0-9, '_' and '-'.");
        }

        return validationErrors;
    }

    // GwiOS sends the token of the satellite to this address, so it must not lead anywhere but to a plain HTTP server.
    private static bool IsValidEndpoint(string endpoint)
        => (endpoint.Length <= MaxTextLength)
            && Uri.TryCreate(endpoint, UriKind.Absolute, out Uri? uri)
            && (uri.Scheme == Uri.UriSchemeHttp)
            && (uri.UserInfo.Length == 0)
            && (uri.AbsolutePath == "/")
            && (uri.Query.Length == 0)
            && (uri.Fragment.Length == 0);

    private static bool IsValidText(string text)
        => (text.Length > 0) && (text.Length <= MaxTextLength) && !text.Any(char.IsControl);

    private static bool AreValidSensors(List<SatelliteSensor> sensors)
        => (sensors.Count <= MaxSensorCount) && sensors.All(IsValidSensor);

    private static bool IsValidSensor(SatelliteSensor sensor)
        => IdentifierPattern().IsMatch(sensor.Type)
            && (sensor.Quantities.Count > 0)
            && (sensor.Quantities.Count <= MaxQuantityCount)
            && sensor.Quantities.All(quantity => IdentifierPattern().IsMatch(quantity));

    private void LogValidationErrors(string satelliteName, List<string> validationErrors)
        => _logger.LogWarning(
            "Satellite ping is invalid.",
            new Dictionary<string, string>
            {
                ["SatelliteName"] = satelliteName,
                ["ValidationErrors"] = string.Join(" ", validationErrors)
            });

    // \z instead of $, which would also accept a trailing line break.
    [GeneratedRegex("^[a-z0-9_-]{1,32}\\z")]
    private static partial Regex IdentifierPattern();
}
