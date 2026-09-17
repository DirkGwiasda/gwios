using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;
using GwiOS.Core.Tests.TestInfrastructure;
using SatellitePresenceManagerUnderTest =
    GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.SatellitePresenceManager;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatellitePresenceManagement.SatellitePresenceManager;

/// <summary>
/// Runs a <c>SatellitePresenceManager</c> against fakes of its collaborators and gives the tests access to them.
/// </summary>
internal sealed class SatellitePresenceManagerProbe
{
    public SatellitePresenceManagerProbe()
    {
        SatellitePresenceManager = new SatellitePresenceManagerUnderTest(Repository, Validator, TimeProvider, Logger);
    }

    /// <summary>
    /// The repository the manager stores presences in.
    /// </summary>
    public SatellitePresenceRepositoryFake Repository { get; } = new();

    /// <summary>
    /// The validator; every presence is valid until the test sets errors.
    /// </summary>
    public SatellitePresenceValidatorFake Validator { get; } = new();

    /// <summary>
    /// The clock of the manager.
    /// </summary>
    public TimeProviderFake TimeProvider { get; } = new();

    /// <summary>
    /// The logger the manager writes to.
    /// </summary>
    public LoggerFake<SatellitePresenceManagerUnderTest> Logger { get; } = new();

    /// <summary>
    /// Creates valid presences.
    /// </summary>
    public TestPresences Presences { get; } = new();

    /// <summary>
    /// The manager under test.
    /// </summary>
    public SatellitePresenceManagerUnderTest SatellitePresenceManager { get; }
}
