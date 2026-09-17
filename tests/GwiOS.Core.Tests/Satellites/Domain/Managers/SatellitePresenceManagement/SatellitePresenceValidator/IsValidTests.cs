using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;
using SatellitePresenceValidatorUnderTest =
    GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.SatellitePresenceValidator;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatellitePresenceManagement.SatellitePresenceValidator;

/// <summary>
/// Covers <c>SatellitePresenceValidator.IsValid</c>.
/// </summary>
public sealed class IsValidTests
{
    private readonly LoggerFake<SatellitePresenceValidatorUnderTest> _logger = new();
    private readonly TestPresences _presences = new();
    private readonly SatellitePresenceValidatorUnderTest _validator;

    public IsValidTests()
    {
        _validator = new SatellitePresenceValidatorUnderTest(_logger);
    }

    [Fact]
    public void AcceptsThePresenceOfTheBme280Satellite()
    {
        bool isValid = _validator.IsValid(_presences.Create(), out List<string> validationErrors);

        Assert.True(isValid);
        Assert.Empty(validationErrors);
        Assert.Empty(_logger.Entries);
    }

    [Theory]
    [InlineData("http://192.168.178.33")]
    [InlineData("http://192.168.178.33/")]
    [InlineData("http://192.168.178.33:8080")]
    [InlineData("http://gwios-keller")]
    public void AcceptsAPlainHttpEndpoint(string endpoint)
    {
        Assert.True(_validator.IsValid(WithEndpoint(endpoint), out _));
    }

    [Theory]
    [InlineData("")]
    [InlineData("192.168.178.33")]
    [InlineData("https://192.168.178.33")]
    [InlineData("ftp://192.168.178.33")]
    [InlineData("http://192.168.178.33/measurements")]
    [InlineData("http://192.168.178.33/?x=1")]
    [InlineData("http://user:secret@192.168.178.33")]
    [InlineData("http://a-very-long-host-name-that-nobody-uses-in-a-home-network.example")]
    public void RejectsAnEndpointThatIsNoPlainHttpAddress(string endpoint)
    {
        bool isValid = _validator.IsValid(WithEndpoint(endpoint), out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Contains("endpoint", Assert.Single(validationErrors));
    }

    [Theory]
    [InlineData("", "0.1.0")]
    [InlineData("ESP32\nDevKit", "0.1.0")]
    [InlineData("ESP32 DevKit C", "")]
    public void RejectsAnEmptyBoardOrFirmwareOrOneWithControlCharacters(string board, string firmware)
    {
        SatellitePresence presence = _presences.Create();
        SatellitePresence changedPresence = new()
        {
            SatelliteName = presence.SatelliteName,
            Endpoint = presence.Endpoint,
            Board = board,
            Firmware = firmware,
            WifiRssiDbm = presence.WifiRssiDbm,
            Sensors = presence.Sensors
        };

        Assert.False(_validator.IsValid(changedPresence, out List<string> validationErrors));
        Assert.Single(validationErrors);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-128)]
    public void RejectsASignalStrengthOutsideTheRange(int wifiRssiDbm)
    {
        SatellitePresence presence = _presences.Create();
        SatellitePresence changedPresence = new()
        {
            SatelliteName = presence.SatelliteName,
            Endpoint = presence.Endpoint,
            Board = presence.Board,
            Firmware = presence.Firmware,
            WifiRssiDbm = wifiRssiDbm,
            Sensors = presence.Sensors
        };

        Assert.False(_validator.IsValid(changedPresence, out _));
    }

    [Theory]
    [InlineData("BME280", "temperature")]
    [InlineData("bme280", "")]
    [InlineData("bme 280", "temperature")]
    public void RejectsASensorWithInvalidTypeOrQuantity(string type, string quantity)
    {
        SatellitePresence presence = WithSensors([new SatelliteSensor { Type = type, Quantities = [quantity] }]);

        Assert.False(_validator.IsValid(presence, out _));
    }

    [Fact]
    public void RejectsASensorWithoutQuantitiesAndMoreThanEightSensors()
    {
        SatelliteSensor sensor = new() { Type = "bme280", Quantities = ["temperature"] };

        Assert.False(_validator.IsValid(WithSensors([new SatelliteSensor { Type = "bme280", Quantities = [] }]), out _));
        Assert.True(_validator.IsValid(WithSensors([.. Enumerable.Repeat(sensor, 8)]), out _));
        Assert.False(_validator.IsValid(WithSensors([.. Enumerable.Repeat(sensor, 9)]), out _));
    }

    [Fact]
    public void AcceptsASatelliteWithoutSensors()
    {
        Assert.True(_validator.IsValid(WithSensors([]), out _));
    }

    [Fact]
    public void LogsAWarningWithTheSatelliteName_WhenThePresenceIsInvalid()
    {
        _validator.IsValid(WithEndpoint("https://x"), out _);

        LogEntry logEntry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Warning, logEntry.LogLevel);
        Assert.Equal("arbeitszimmer", logEntry.ContextData["SatelliteName"]);
    }

    private SatellitePresence WithEndpoint(string endpoint)
    {
        SatellitePresence presence = _presences.Create();
        return new SatellitePresence
        {
            SatelliteName = presence.SatelliteName,
            Endpoint = endpoint,
            Board = presence.Board,
            Firmware = presence.Firmware,
            WifiRssiDbm = presence.WifiRssiDbm,
            Sensors = presence.Sensors
        };
    }

    private SatellitePresence WithSensors(List<SatelliteSensor> sensors)
    {
        SatellitePresence presence = _presences.Create();
        return new SatellitePresence
        {
            SatelliteName = presence.SatelliteName,
            Endpoint = presence.Endpoint,
            Board = presence.Board,
            Firmware = presence.Firmware,
            WifiRssiDbm = presence.WifiRssiDbm,
            Sensors = sensors
        };
    }
}
