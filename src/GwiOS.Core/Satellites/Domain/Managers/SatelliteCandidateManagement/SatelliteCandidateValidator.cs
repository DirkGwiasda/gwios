using System.Text.RegularExpressions;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement;

internal sealed partial class SatelliteCandidateValidator(ILogger<SatelliteCandidateValidator> logger)
    : ISatelliteCandidateValidator
{
    private const int MaxTextLength = 64;
    private const int MinWifiRssiDbm = -127;
    private const int MaxWifiRssiDbm = 0;

    private readonly ILogger<SatelliteCandidateValidator> _logger = logger;

    public bool IsValid(SatelliteCandidate candidate, string token)
    {
        if (AreValuesValid(candidate, token))
        {
            return true;
        }

        // Neither name nor token are logged: anyone who reaches the endpoint can send arbitrary values.
        _logger.LogWarning("Ping of unknown satellite is invalid.");
        return false;
    }

    private static bool AreValuesValid(SatelliteCandidate candidate, string token)
        => NamePattern().IsMatch(candidate.Name)
            && TokenPattern().IsMatch(token)
            && IsValidEndpoint(candidate.Endpoint)
            && IsValidText(candidate.Board)
            && IsValidText(candidate.Firmware)
            && (candidate.WifiRssiDbm >= MinWifiRssiDbm)
            && (candidate.WifiRssiDbm <= MaxWifiRssiDbm);

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

    // \z instead of $, which would also accept a trailing line break.
    [GeneratedRegex("^[a-z0-9-]{1,32}\\z")]
    private static partial Regex NamePattern();

    [GeneratedRegex("^[A-Za-z0-9_-]{32,128}\\z")]
    private static partial Regex TokenPattern();
}
