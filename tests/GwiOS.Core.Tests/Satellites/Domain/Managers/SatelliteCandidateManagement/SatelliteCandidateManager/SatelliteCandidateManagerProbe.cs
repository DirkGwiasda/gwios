using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;
using GwiOS.Core.Tests.TestInfrastructure;
using SatelliteCandidateManagerUnderTest =
    GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.SatelliteCandidateManager;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.SatelliteCandidateManager;

/// <summary>
/// Runs a <c>SatelliteCandidateManager</c> against fakes of its collaborators and gives the tests access to them.
/// </summary>
internal sealed class SatelliteCandidateManagerProbe
{
    public SatelliteCandidateManagerProbe()
    {
        SatelliteCandidateManager = new SatelliteCandidateManagerUnderTest(Repository, Validator, TimeProvider, Logger);
    }

    /// <summary>
    /// The repository the manager keeps candidates in.
    /// </summary>
    public SatelliteCandidateRepositoryFake Repository { get; } = new();

    /// <summary>
    /// The validator; every candidate is valid until the test says otherwise.
    /// </summary>
    public SatelliteCandidateValidatorFake Validator { get; } = new();

    /// <summary>
    /// The clock of the manager.
    /// </summary>
    public TimeProviderFake TimeProvider { get; } = new();

    /// <summary>
    /// The logger the manager writes to.
    /// </summary>
    public LoggerFake<SatelliteCandidateManagerUnderTest> Logger { get; } = new();

    /// <summary>
    /// Creates candidates.
    /// </summary>
    public TestCandidates Candidates { get; } = new();

    /// <summary>
    /// The manager under test.
    /// </summary>
    public SatelliteCandidateManagerUnderTest SatelliteCandidateManager { get; }

    /// <summary>
    /// Moves the clock forward by the given number of minutes.
    /// </summary>
    public void AdvanceMinutes(double minutes)
        => TimeProvider.UtcNow += TimeSpan.FromMinutes(minutes);
}
