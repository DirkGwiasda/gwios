using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.Contracts;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.SatelliteManager;

/// <summary>
/// Covers <c>SatelliteManager.CreateSatelliteWithTokenAsync</c>.
/// </summary>
public sealed class CreateSatelliteWithTokenAsyncTests
{
    private readonly SatelliteManagerProbe _probe = new();

    [Fact]
    public async Task StoresTheSatelliteWithTheGivenTokenEncrypted()
    {
        await _probe.SatelliteManager.CreateSatelliteWithTokenAsync(
            "keller",
            "Keller",
            "adopted-token",
            TestContext.Current.CancellationToken);

        Satellite satellite = Assert.Single(_probe.SatelliteRepository.Satellites);
        Assert.Equal("keller", satellite.Name);
        Assert.Equal("Keller", satellite.DisplayName);
        Assert.Equal(SatelliteTokenProtectorFake.Prefix + "adopted-token", satellite.ProtectedToken);
    }

    [Fact]
    public async Task ValidatesTheToken()
    {
        await _probe.SatelliteManager.CreateSatelliteWithTokenAsync(
            "keller",
            null,
            "adopted-token",
            TestContext.Current.CancellationToken);

        Assert.Equal(["adopted-token"], _probe.SatelliteValidator.ValidatedTokens);
    }

    [Fact]
    public async Task ThrowsSatelliteValidationExceptionAndStoresNothing_WhenTheTokenIsInvalid()
    {
        _probe.SatelliteValidator.IsTokenValid = false;

        SatelliteValidationException exception = await Assert.ThrowsAsync<SatelliteValidationException>(
            () => _probe.SatelliteManager.CreateSatelliteWithTokenAsync(
                "keller",
                null,
                "short",
                TestContext.Current.CancellationToken));

        Assert.Equal("keller", exception.SatelliteName);
        Assert.Contains("token", Assert.Single(exception.ValidationErrors));
        Assert.Empty(_probe.SatelliteRepository.Satellites);
    }

    [Fact]
    public async Task ThrowsSatelliteValidationException_WhenTheSatelliteIsInvalid()
    {
        _probe.SatelliteValidator.ValidationErrors = ["Invalid display name."];

        await Assert.ThrowsAsync<SatelliteValidationException>(
            () => _probe.SatelliteManager.CreateSatelliteWithTokenAsync(
                "keller",
                "x",
                "adopted-token",
                TestContext.Current.CancellationToken));
    }
}
