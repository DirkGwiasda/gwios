using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Exceptions;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.MeasurementManagement.MeasurementManager;

/// <summary>
/// Covers <c>MeasurementManager.RecordReading</c>.
/// </summary>
public sealed class RecordReadingTests
{
    private readonly MeasurementManagerProbe _probe = new();

    [Fact]
    public void AddsOneSamplePerMeasurementForTheSatellite()
    {
        _probe.MeasurementManager.RecordReading("keller", _probe.Readings.Create(temperature: 21.5));

        Assert.Equal(
            [("keller", "temperature", 21.5), ("keller", "humidity", 50.64), ("keller", "pressure", 996.37)],
            _probe.Repository.AddedSamples.Select(added => (added.SatelliteName, added.Measurement.Quantity, added.Sample.Value)));
    }

    [Fact]
    public void StampsTheSamplesWithTheFetchTimeAndTheMeasurementTimeDerivedFromTheAge()
    {
        _probe.MeasurementManager.RecordReading("keller", _probe.Readings.Create(ageSeconds: 4));

        DateTime now = _probe.TimeProvider.UtcNow.UtcDateTime;
        Assert.All(_probe.Repository.AddedSamples, added =>
        {
            Assert.Equal(now, added.Sample.FetchedAt);
            Assert.Equal(now.AddSeconds(-4), added.Sample.MeasuredAt);
        });
    }

    [Fact]
    public void ThrowsMeasurementValidationExceptionAndAddsNothing_WhenTheReadingIsInvalid()
    {
        _probe.Validator.ValidationErrors = ["Invalid value."];

        MeasurementValidationException exception = Assert.Throws<MeasurementValidationException>(
            () => _probe.MeasurementManager.RecordReading("keller", _probe.Readings.Create()));

        Assert.Equal("keller", exception.SatelliteName);
        Assert.Equal(["Invalid value."], exception.ValidationErrors);
        Assert.Empty(_probe.Repository.AddedSamples);
    }
}
