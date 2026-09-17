using System.Text.RegularExpressions;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement;

internal sealed partial class SatelliteValidator(ILogger<SatelliteValidator> logger) : ISatelliteValidator
{
    private const int MaxDisplayNameLength = 64;

    private readonly ILogger<SatelliteValidator> _logger = logger;

    public bool IsValid(Satellite satellite, out List<string> validationErrors)
    {
        validationErrors = CollectValidationErrors(satellite);
        if (validationErrors.Count == 0)
        {
            return true;
        }

        LogValidationErrors(validationErrors);
        return false;
    }

    public bool IsValidToken(string token)
    {
        if (TokenPattern().IsMatch(token))
        {
            return true;
        }

        _logger.LogWarning("Satellite token has an invalid format.");
        return false;
    }

    private static List<string> CollectValidationErrors(Satellite satellite)
    {
        List<string> validationErrors = [];
        if (!NamePattern().IsMatch(satellite.Name))
        {
            validationErrors.Add("The name must consist of 1 to 32 characters out of a-z, 0-9 and '-'.");
        }

        if ((satellite.DisplayName is not null) && !IsValidDisplayName(satellite.DisplayName))
        {
            validationErrors.Add(
                $"The display name must not be empty, must not exceed {MaxDisplayNameLength} characters and must not "
                + "contain control characters; it is null if the satellite has none.");
        }

        return validationErrors;
    }

    private static bool IsValidDisplayName(string displayName)
        => !string.IsNullOrWhiteSpace(displayName)
            && (displayName.Length <= MaxDisplayNameLength)
            && !displayName.Any(char.IsControl);

    // Only the violated rules are logged, not the rejected values.
    private void LogValidationErrors(List<string> validationErrors)
        => _logger.LogWarning(
            "Satellite is invalid.",
            new Dictionary<string, string> { ["ValidationErrors"] = string.Join(" ", validationErrors) });

    // \z instead of $, which would also accept a trailing line break.
    [GeneratedRegex("^[a-z0-9-]{1,32}\\z")]
    private static partial Regex NamePattern();

    [GeneratedRegex("^[A-Za-z0-9_-]{32,128}\\z")]
    private static partial Regex TokenPattern();
}
