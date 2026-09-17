using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Infrastructure;

/// <summary>
/// Keeps the measurement histories of the satellites in memory for the lifetime of the application. Registered as
/// singleton.
/// </summary>
internal sealed class MeasurementHistoryInMemoryRepository(ILogger<MeasurementHistoryInMemoryRepository> logger)
    : IMeasurementHistoryRepository
{
    private const int RecentSampleCount = 10;
    private const int LongTermSampleInterval = 10;
    private const int LongTermSampleCount = 100;

    private readonly Lock _lock = new();
    private readonly Dictionary<HistoryKey, StoredHistory> _histories = [];
    private readonly ILogger<MeasurementHistoryInMemoryRepository> _logger = logger;

    public void AddSample(string satelliteName, Measurement measurement, MeasurementSample sample)
    {
        HistoryKey key = new(satelliteName, measurement.Sensor, measurement.Quantity);
        lock (_lock)
        {
            StoredHistory history = GetOrCreateHistory(key);
            history.Unit = measurement.Unit;
            AddToRecentSamples(history, sample);
            AddToLongTermSamples(history, sample);
        }
    }

    public List<MeasurementHistory> GetBySatelliteName(string satelliteName)
    {
        lock (_lock)
        {
            return
            [
                .. _histories
                    .Where(entry => entry.Key.SatelliteName == satelliteName)
                    .OrderBy(entry => entry.Key.Sensor, StringComparer.Ordinal)
                    .ThenBy(entry => entry.Key.Quantity, StringComparer.Ordinal)
                    .Select(entry => CopyHistory(entry.Key, entry.Value))
            ];
        }
    }

    private StoredHistory GetOrCreateHistory(HistoryKey key)
    {
        if (_histories.TryGetValue(key, out StoredHistory? history))
        {
            return history;
        }

        history = new StoredHistory();
        _histories.Add(key, history);
        _logger.LogDebug(
            "Measurement history created.",
            new Dictionary<string, string>
            {
                ["SatelliteName"] = key.SatelliteName,
                ["Sensor"] = key.Sensor,
                ["Quantity"] = key.Quantity
            });
        return history;
    }

    private static void AddToRecentSamples(StoredHistory history, MeasurementSample sample)
    {
        history.RecentSamples.Enqueue(sample);
        if (history.RecentSamples.Count > RecentSampleCount)
        {
            history.RecentSamples.Dequeue();
        }
    }

    // The first sample goes into the long history, too, so it is not empty during the first minutes.
    private static void AddToLongTermSamples(StoredHistory history, MeasurementSample sample)
    {
        if ((history.AddedSampleCount % LongTermSampleInterval) == 0)
        {
            history.LongTermSamples.Enqueue(sample);
            if (history.LongTermSamples.Count > LongTermSampleCount)
            {
                history.LongTermSamples.Dequeue();
            }
        }

        history.AddedSampleCount++;
    }

    private static MeasurementHistory CopyHistory(HistoryKey key, StoredHistory history)
        => new()
        {
            SatelliteName = key.SatelliteName,
            Sensor = key.Sensor,
            Quantity = key.Quantity,
            Unit = history.Unit,
            RecentSamples = [.. history.RecentSamples],
            LongTermSamples = [.. history.LongTermSamples]
        };

    private sealed record HistoryKey(string SatelliteName, string Sensor, string Quantity);

    private sealed class StoredHistory
    {
        public string Unit { get; set; } = string.Empty;

        public long AddedSampleCount { get; set; }

        public Queue<MeasurementSample> RecentSamples { get; } = new();

        public Queue<MeasurementSample> LongTermSamples { get; } = new();
    }
}
