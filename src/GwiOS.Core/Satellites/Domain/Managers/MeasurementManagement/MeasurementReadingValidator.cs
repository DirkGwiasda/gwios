using System.Text.RegularExpressions;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement;

internal sealed partial class MeasurementReadingValidator(ILogger<MeasurementReadingValidator> logger)
    : IMeasurementReadingValidator
{
    private const int MaxMeasurementCount = 32;
    private const int MaxUnitLength = 16;
    private const int MaxAgeSeconds = 24 * 60 * 60;

    private readonly ILogger<MeasurementReadingValidator> _logger = logger;

    public bool IsValid(string satelliteName, MeasurementReading reading, out List<string> validationErrors)
    {
        validationErrors = CollectValidationErrors(reading);
        if (validationErrors.Count == 0)
        {
            return true;
        }

        LogValidationErrors(satelliteName, validationErrors);
        return false;
    }

    private static List<string> CollectValidationErrors(MeasurementReading reading)
    {
        List<string> validationErrors = [];
        if (reading.Measurements.Count > MaxMeasurementCount)
        {
            validationErrors.Add($"A reading must not contain more than {MaxMeasurementCount} measurements.");
        }

        if (!reading.Measurements.All(IsValidMeasurement))
        {
            validationErrors.Add(
                "Sensor and quantity must consist of 1 to 32 characters out of a-z, 0-9, '_' and '-', the unit of at "
                + $"most {MaxUnitLength} characters without control characters, and the value must be a finite number.");
        }

        if ((reading.AgeSeconds < 0) || (reading.AgeSeconds > MaxAgeSeconds))
        {
            validationErrors.Add($"The age must lie between 0 and {MaxAgeSeconds} seconds.");
        }

        return validationErrors;
    }

    private static bool IsValidMeasurement(Measurement measurement)
        => IdentifierPattern().IsMatch(measurement.Sensor)
            && IdentifierPattern().IsMatch(measurement.Quantity)
            && (measurement.Unit.Length <= MaxUnitLength)
            && !measurement.Unit.Any(char.IsControl)
            && double.IsFinite(measurement.Value);

    private void LogValidationErrors(string satelliteName, List<string> validationErrors)
        => _logger.LogWarning(
            "Measurement reading is invalid.",
            new Dictionary<string, string>
            {
                ["SatelliteName"] = satelliteName,
                ["ValidationErrors"] = string.Join(" ", validationErrors)
            });

    // \z instead of $, which would also accept a trailing line break.
    [GeneratedRegex("^[a-z0-9_-]{1,32}\\z")]
    private static partial Regex IdentifierPattern();
}
