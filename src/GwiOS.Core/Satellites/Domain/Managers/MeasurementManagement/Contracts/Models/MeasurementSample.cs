namespace GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;

/// <summary>
/// A value in the history of a quantity, with the times it was measured and fetched.
/// </summary>
/// <param name="Value">The measured value, in the unit of its history.</param>
/// <param name="MeasuredAt">Point in time the satellite measured the value, in UTC.</param>
/// <param name="FetchedAt">Point in time GwiOS fetched the value, in UTC.</param>
public record MeasurementSample(double Value, DateTime MeasuredAt, DateTime FetchedAt);
