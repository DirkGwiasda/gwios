using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.MeasurementManagement.MeasurementManager;

/// <summary>
/// Covers <c>MeasurementManager.FetchReadingAsync</c>.
/// </summary>
public sealed class FetchReadingAsyncTests
{
    private readonly MeasurementManagerProbe _probe = new();

    [Fact]
    public async Task ReturnsTheReadingOfTheSatellite()
    {
        MeasurementReading expectedReading = _probe.Readings.Create();
        _probe.Client.Reading = expectedReading;

        MeasurementReading reading = await _probe.MeasurementManager.FetchReadingAsync(
            "keller",
            "http://192.168.178.33",
            "token",
            TestContext.Current.CancellationToken);

        Assert.Same(expectedReading, reading);
        Assert.Equal(("keller", "http://192.168.178.33", "token"), Assert.Single(_probe.Client.Requests));
    }

    [Fact]
    public async Task RethrowsAndLogsAWarning_WhenTheFetchFails()
    {
        _probe.Client.IsReachable = false;

        await Assert.ThrowsAsync<MeasurementFetchException>(
            () => _probe.MeasurementManager.FetchReadingAsync(
                "keller",
                "http://192.168.178.33",
                "token",
                TestContext.Current.CancellationToken));

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Warning, logEntry.LogLevel);
        Assert.Equal("keller", logEntry.ContextData["SatelliteName"]);
        Assert.DoesNotContain("token", logEntry.Message);
    }

    [Fact]
    public async Task LogsNothing_WhenTheFetchSucceeds()
    {
        await _probe.MeasurementManager.FetchReadingAsync(
            "keller",
            "http://192.168.178.33",
            "token",
            TestContext.Current.CancellationToken);

        Assert.Empty(_probe.Logger.Entries);
    }
}
