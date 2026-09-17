using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using MeasurementHistoryInMemoryRepositoryUnderTest =
    GwiOS.Core.Satellites.Infrastructure.MeasurementHistoryInMemoryRepository;

namespace GwiOS.Core.Tests.Satellites.Infrastructure.MeasurementHistoryInMemoryRepository;

/// <summary>
/// Covers <c>MeasurementHistoryInMemoryRepository.AddSample</c>.
/// </summary>
public sealed class AddSampleTests
{
    private static readonly DateTime StartTime = new(2026, 9, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly LoggerFake<MeasurementHistoryInMemoryRepositoryUnderTest> _logger = new();
    private readonly MeasurementHistoryInMemoryRepositoryUnderTest _repository;

    public AddSampleTests()
    {
        _repository = new MeasurementHistoryInMemoryRepositoryUnderTest(_logger);
    }

    [Fact]
    public void KeepsTheLastTenSamplesInTheShortHistory()
    {
        AddTemperatures(count: 12);

        MeasurementHistory history = GetTemperatureHistory();
        Assert.Equal([3.0, 4, 5, 6, 7, 8, 9, 10, 11, 12], history.RecentSamples.Select(sample => sample.Value));
    }

    [Fact]
    public void KeepsEveryTenthSampleStartingWithTheFirstInTheLongHistory()
    {
        AddTemperatures(count: 25);

        MeasurementHistory history = GetTemperatureHistory();
        Assert.Equal([1.0, 11, 21], history.LongTermSamples.Select(sample => sample.Value));
    }

    [Fact]
    public void KeepsTheLastHundredSamplesInTheLongHistory()
    {
        AddTemperatures(count: 1010);

        MeasurementHistory history = GetTemperatureHistory();
        Assert.Equal(100, history.LongTermSamples.Count);
        Assert.Equal(11, history.LongTermSamples[0].Value);
        Assert.Equal(1001, history.LongTermSamples[^1].Value);
    }

    [Fact]
    public void KeepsSeparateHistoriesPerSatelliteSensorAndQuantity()
    {
        _repository.AddSample("keller", CreateMeasurement("temperature", "°C"), CreateSample(20));
        _repository.AddSample("keller", CreateMeasurement("humidity", "%"), CreateSample(50));
        _repository.AddSample("garage", CreateMeasurement("temperature", "°C"), CreateSample(10));

        Assert.Equal(2, _repository.GetBySatelliteName("keller").Count);
        Assert.Equal(10, Assert.Single(_repository.GetBySatelliteName("garage")).RecentSamples.Single().Value);
    }

    [Fact]
    public void TakesTheUnitOfTheLatestMeasurement()
    {
        _repository.AddSample("keller", CreateMeasurement("temperature", "C"), CreateSample(20));
        _repository.AddSample("keller", CreateMeasurement("temperature", "°C"), CreateSample(21));

        Assert.Equal("°C", GetTemperatureHistory().Unit);
    }

    [Fact]
    public void LogsOnlyTheCreationOfAHistory()
    {
        AddTemperatures(count: 3);

        Assert.Equal("keller", Assert.Single(_logger.Entries).ContextData["SatelliteName"]);
    }

    private void AddTemperatures(int count)
    {
        for (int number = 1; number <= count; number++)
        {
            _repository.AddSample("keller", CreateMeasurement("temperature", "°C"), CreateSample(number));
        }
    }

    private MeasurementHistory GetTemperatureHistory()
        => _repository.GetBySatelliteName("keller").Single(history => history.Quantity == "temperature");

    private static Measurement CreateMeasurement(string quantity, string unit)
        => new() { Sensor = "bme280", Quantity = quantity, Unit = unit, Value = 0 };

    private static MeasurementSample CreateSample(double value)
        => new(value, StartTime.AddSeconds(value * 30), StartTime.AddSeconds(value * 30));
}
