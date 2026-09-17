using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using MeasurementHistoryInMemoryRepositoryUnderTest =
    GwiOS.Core.Satellites.Infrastructure.MeasurementHistoryInMemoryRepository;

namespace GwiOS.Core.Tests.Satellites.Infrastructure.MeasurementHistoryInMemoryRepository;

/// <summary>
/// Covers <c>MeasurementHistoryInMemoryRepository.GetBySatelliteName</c>.
/// </summary>
public sealed class GetBySatelliteNameTests
{
    private static readonly MeasurementSample Sample = new(20, DateTime.UtcNow, DateTime.UtcNow);

    private readonly MeasurementHistoryInMemoryRepositoryUnderTest _repository =
        new(new LoggerFake<MeasurementHistoryInMemoryRepositoryUnderTest>());

    [Fact]
    public void ReturnsTheHistoriesOrderedBySensorAndQuantity()
    {
        _repository.AddSample("keller", CreateMeasurement("sht31", "humidity"), Sample);
        _repository.AddSample("keller", CreateMeasurement("bme280", "temperature"), Sample);
        _repository.AddSample("keller", CreateMeasurement("bme280", "humidity"), Sample);

        List<MeasurementHistory> histories = _repository.GetBySatelliteName("keller");

        Assert.Equal(
            [("bme280", "humidity"), ("bme280", "temperature"), ("sht31", "humidity")],
            histories.Select(history => (history.Sensor, history.Quantity)));
        Assert.All(histories, history => Assert.Equal("keller", history.SatelliteName));
    }

    [Fact]
    public void ReturnsCopiesThatDoNotChangeWithLaterSamples()
    {
        _repository.AddSample("keller", CreateMeasurement("bme280", "temperature"), Sample);
        MeasurementHistory history = Assert.Single(_repository.GetBySatelliteName("keller"));

        _repository.AddSample("keller", CreateMeasurement("bme280", "temperature"), Sample);

        Assert.Single(history.RecentSamples);
    }

    [Fact]
    public void ReturnsAnEmptyList_ForASatelliteWithoutHistories()
    {
        Assert.Empty(_repository.GetBySatelliteName("keller"));
    }

    private static Measurement CreateMeasurement(string sensor, string quantity)
        => new() { Sensor = sensor, Quantity = quantity, Unit = "x", Value = 0 };
}
