using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Tests.TestInfrastructure;
using SatelliteManagerUnderTest = GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.SatelliteManager;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.SatelliteManager;

/// <summary>
/// Runs a <c>SatelliteManager</c> against fakes of all its collaborators and gives the tests access to them.
/// </summary>
internal sealed class SatelliteManagerProbe
{
    public SatelliteManagerProbe()
    {
        SatelliteManager = new SatelliteManagerUnderTest(
            SatelliteRepository,
            SatelliteValidator,
            TokenGenerator,
            TokenProtector,
            TimeProvider,
            Logger);
    }

    /// <summary>
    /// The in-memory repository the manager stores its satellites in.
    /// </summary>
    public SatelliteRepositoryFake SatelliteRepository { get; } = new();

    /// <summary>
    /// The validator the manager checks satellites and tokens with; everything is valid until the test says otherwise.
    /// </summary>
    public SatelliteValidatorFake SatelliteValidator { get; } = new();

    /// <summary>
    /// The generator of predictable tokens.
    /// </summary>
    public SatelliteTokenGeneratorFake TokenGenerator { get; } = new();

    /// <summary>
    /// The protector that marks encrypted tokens with a prefix.
    /// </summary>
    public SatelliteTokenProtectorFake TokenProtector { get; } = new();

    /// <summary>
    /// The clock of the manager.
    /// </summary>
    public TimeProviderFake TimeProvider { get; } = new();

    /// <summary>
    /// The logger the manager writes to.
    /// </summary>
    public LoggerFake<SatelliteManagerUnderTest> Logger { get; } = new();

    /// <summary>
    /// The manager under test.
    /// </summary>
    public SatelliteManagerUnderTest SatelliteManager { get; }

    /// <summary>
    /// Stores a satellite with the given name and token directly, as if it had been created before the test.
    /// </summary>
    public Satellite AddSatellite(string name, string? token = "stored-token", bool isLocked = false)
    {
        Satellite satellite = new()
        {
            Name = name,
            IsLocked = isLocked,
            ProtectedToken = token is null ? null : TokenProtector.Protect(token)
        };
        SatelliteRepository.Add(satellite);
        return satellite;
    }

    /// <summary>
    /// Returns the stored satellite with the given name.
    /// </summary>
    public Satellite GetStoredSatellite(string name)
        => SatelliteRepository.Satellites.Single(satellite => satellite.Name == name);
}
