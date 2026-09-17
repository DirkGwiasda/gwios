using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.MeasurementManagement.Contracts;
using MeasurementReadingValidatorUnderTest =
    GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.MeasurementReadingValidator;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.MeasurementManagement.MeasurementReadingValidator;

/// <summary>
/// Covers <c>MeasurementReadingValidator.IsValid</c>.
/// </summary>
public sealed class IsValidTests
{
    private readonly LoggerFake<MeasurementReadingValidatorUnderTest> _logger = new();
    private readonly TestReadings _readings = new();
    private readonly MeasurementReadingValidatorUnderTest _validator;

    public IsValidTests()
    {
        _validator = new MeasurementReadingValidatorUnderTest(_logger);
    }

    [Fact]
    public void AcceptsTheReadingOfTheBme280Satellite()
    {
        bool isValid = _validator.IsValid("keller", _readings.Create(), out List<string> validationErrors);

        Assert.True(isValid);
        Assert.Empty(validationErrors);
    }

    [Fact]
    public void AcceptsAReadingWithoutMeasurements()
    {
        Assert.True(_validator.IsValid("keller", new MeasurementReading { Measurements = [] }, out _));
    }

    [Theory]
    [InlineData("BME280", "temperature", "°C", 20.0)]
    [InlineData("bme280", "", "°C", 20.0)]
    [InlineData("bme280", "temperature", "degrees of celsius", 20.0)]
    [InlineData("bme280", "temperature", "°C\n", 20.0)]
    [InlineData("bme280", "temperature", "°C", double.NaN)]
    [InlineData("bme280", "temperature", "°C", double.PositiveInfinity)]
    public void RejectsAnInvalidMeasurement(string sensor, string quantity, string unit, double value)
    {
        MeasurementReading reading = new()
        {
            Measurements = [new Measurement { Sensor = sensor, Quantity = quantity, Unit = unit, Value = value }]
        };

        Assert.False(_validator.IsValid("keller", reading, out List<string> validationErrors));
        Assert.Single(validationErrors);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(86401)]
    public void RejectsAnAgeOutsideOneDay(int ageSeconds)
    {
        Assert.False(_validator.IsValid("keller", _readings.Create(ageSeconds: ageSeconds), out _));
    }

    [Fact]
    public void RejectsMoreThan32Measurements()
    {
        Measurement measurement = new() { Sensor = "bme280", Quantity = "temperature", Unit = "°C", Value = 20 };

        Assert.True(_validator.IsValid("keller", new MeasurementReading { Measurements = [.. Enumerable.Repeat(measurement, 32)] }, out _));
        Assert.False(_validator.IsValid("keller", new MeasurementReading { Measurements = [.. Enumerable.Repeat(measurement, 33)] }, out _));
    }

    [Fact]
    public void LogsAWarningWithTheSatelliteName_WhenTheReadingIsInvalid()
    {
        _validator.IsValid("keller", _readings.Create(ageSeconds: -1), out _);

        LogEntry logEntry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Warning, logEntry.LogLevel);
        Assert.Equal("keller", logEntry.ContextData["SatelliteName"]);
    }
}
