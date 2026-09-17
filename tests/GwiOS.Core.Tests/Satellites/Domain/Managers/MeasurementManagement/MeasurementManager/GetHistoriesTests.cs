using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.MeasurementManagement.MeasurementManager;

/// <summary>
/// Covers <c>MeasurementManager.GetHistories</c>.
/// </summary>
public sealed class GetHistoriesTests
{
    private readonly MeasurementManagerProbe _probe = new();

    [Fact]
    public void ReturnsTheHistoriesOfTheSatellite()
    {
        MeasurementHistory history = new()
        {
            SatelliteName = "keller",
            Sensor = "bme280",
            Quantity = "temperature",
            Unit = "°C",
            RecentSamples = [],
            LongTermSamples = []
        };
        _probe.Repository.Histories = [history];

        List<MeasurementHistory> histories = _probe.MeasurementManager.GetHistories("keller");

        Assert.Same(history, Assert.Single(histories));
        Assert.Empty(_probe.MeasurementManager.GetHistories("garage"));
    }
}
