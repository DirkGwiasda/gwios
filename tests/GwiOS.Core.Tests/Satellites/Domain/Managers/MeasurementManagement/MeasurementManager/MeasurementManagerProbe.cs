using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.MeasurementManagement.Contracts;
using GwiOS.Core.Tests.TestInfrastructure;
using MeasurementManagerUnderTest = GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.MeasurementManager;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.MeasurementManagement.MeasurementManager;

/// <summary>
/// Runs a <c>MeasurementManager</c> against fakes of its collaborators and gives the tests access to them.
/// </summary>
internal sealed class MeasurementManagerProbe
{
    public MeasurementManagerProbe()
    {
        MeasurementManager = new MeasurementManagerUnderTest(Client, Repository, Validator, TimeProvider, Logger);
    }

    /// <summary>
    /// The client the manager fetches readings with.
    /// </summary>
    public SatelliteMeasurementClientFake Client { get; } = new();

    /// <summary>
    /// The repository the manager adds samples to.
    /// </summary>
    public MeasurementHistoryRepositoryFake Repository { get; } = new();

    /// <summary>
    /// The validator; every reading is valid until the test sets errors.
    /// </summary>
    public MeasurementReadingValidatorFake Validator { get; } = new();

    /// <summary>
    /// The clock of the manager.
    /// </summary>
    public TimeProviderFake TimeProvider { get; } = new();

    /// <summary>
    /// The logger the manager writes to.
    /// </summary>
    public LoggerFake<MeasurementManagerUnderTest> Logger { get; } = new();

    /// <summary>
    /// Creates readings.
    /// </summary>
    public TestReadings Readings { get; } = new();

    /// <summary>
    /// The manager under test.
    /// </summary>
    public MeasurementManagerUnderTest MeasurementManager { get; }
}
