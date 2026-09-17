using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.MeasurementManagement.Contracts;

/// <summary>
/// Stands in for <see cref="IMeasurementReadingValidator"/>. Every reading is valid until a test sets
/// <see cref="ValidationErrors"/>.
/// </summary>
public sealed class MeasurementReadingValidatorFake : IMeasurementReadingValidator
{
    /// <summary>
    /// The validation errors reported for every reading. Empty by default, which makes every reading valid.
    /// </summary>
    public List<string> ValidationErrors { get; set; } = [];

    public bool IsValid(string satelliteName, MeasurementReading reading, out List<string> validationErrors)
    {
        validationErrors = [.. ValidationErrors];
        return validationErrors.Count == 0;
    }
}
