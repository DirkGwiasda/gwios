using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts;

/// <summary>
/// Checks a measurement reading fetched from a satellite.
/// </summary>
public interface IMeasurementReadingValidator
{
    /// <summary>
    /// Returns whether the given reading of the satellite with the given name is valid.
    /// <paramref name="validationErrors"/> receives one message per violated rule; it is empty if the reading is valid.
    /// </summary>
    bool IsValid(string satelliteName, MeasurementReading reading, out List<string> validationErrors);
}
