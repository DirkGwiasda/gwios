using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement;

internal sealed class MeasurementManager(
    ISatelliteMeasurementClient satelliteMeasurementClient,
    IMeasurementHistoryRepository measurementHistoryRepository,
    IMeasurementReadingValidator measurementReadingValidator,
    TimeProvider timeProvider,
    ILogger<MeasurementManager> logger) : IMeasurementManager
{
    private readonly ISatelliteMeasurementClient _satelliteMeasurementClient = satelliteMeasurementClient;
    private readonly IMeasurementHistoryRepository _measurementHistoryRepository = measurementHistoryRepository;
    private readonly IMeasurementReadingValidator _measurementReadingValidator = measurementReadingValidator;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<MeasurementManager> _logger = logger;

    public async Task<MeasurementReading> FetchReadingAsync(
        string satelliteName,
        string endpoint,
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _satelliteMeasurementClient.GetReadingAsync(satelliteName, endpoint, token, cancellationToken);
        }
        catch (MeasurementFetchException exception)
        {
            LogFetchFailure(exception);
            throw;
        }
    }

    public void RecordReading(string satelliteName, MeasurementReading reading)
    {
        ThrowIfInvalid(satelliteName, reading);
        DateTime fetchedAt = _timeProvider.GetUtcNow().UtcDateTime;
        DateTime measuredAt = fetchedAt.AddSeconds(-reading.AgeSeconds);
        foreach (Measurement measurement in reading.Measurements)
        {
            _measurementHistoryRepository.AddSample(
                satelliteName,
                measurement,
                new MeasurementSample(measurement.Value, measuredAt, fetchedAt));
        }
    }

    public List<MeasurementHistory> GetHistories(string satelliteName)
        => _measurementHistoryRepository.GetBySatelliteName(satelliteName);

    private void ThrowIfInvalid(string satelliteName, MeasurementReading reading)
    {
        if (!_measurementReadingValidator.IsValid(satelliteName, reading, out List<string> validationErrors))
        {
            throw new MeasurementValidationException(satelliteName, validationErrors);
        }
    }

    // Only online satellites are fetched, so a failure is worth a warning; successful fetches are not logged, because
    // they happen every 30 seconds per satellite.
    private void LogFetchFailure(MeasurementFetchException exception)
        => _logger.LogWarning(
            exception.Message,
            new Dictionary<string, string> { ["SatelliteName"] = exception.SatelliteName });
}
