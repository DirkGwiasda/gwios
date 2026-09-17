using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.Contracts;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.SatelliteManager;

/// <summary>
/// Covers <c>SatelliteManager.CreateSatelliteAsync</c>.
/// </summary>
public sealed class CreateSatelliteAsyncTests
{
    private readonly SatelliteManagerProbe _probe = new();

    [Fact]
    public async Task ReturnsTheNewTokenInPlainText()
    {
        string token = await _probe.SatelliteManager.CreateSatelliteAsync(
            "keller",
            null,
            TestContext.Current.CancellationToken);

        Assert.Equal("generated-token-1", token);
    }

    [Fact]
    public async Task StoresTheSatelliteWithTheEncryptedToken()
    {
        await _probe.SatelliteManager.CreateSatelliteAsync("keller", "Keller", TestContext.Current.CancellationToken);

        Satellite satellite = Assert.Single(_probe.SatelliteRepository.Satellites);
        Assert.Equal("keller", satellite.Name);
        Assert.Equal("Keller", satellite.DisplayName);
        Assert.False(satellite.IsLocked);
        Assert.Equal(SatelliteTokenProtectorFake.Prefix + "generated-token-1", satellite.ProtectedToken);
        Assert.Equal(_probe.TimeProvider.UtcNow.UtcDateTime, satellite.CreatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task StoresNoDisplayName_WhenItIsEmpty(string? displayName)
    {
        await _probe.SatelliteManager.CreateSatelliteAsync("keller", displayName, TestContext.Current.CancellationToken);

        Assert.Null(Assert.Single(_probe.SatelliteRepository.Satellites).DisplayName);
    }

    [Fact]
    public async Task TrimsTheDisplayName()
    {
        await _probe.SatelliteManager.CreateSatelliteAsync("keller", "  Keller  ", TestContext.Current.CancellationToken);

        Assert.Equal("Keller", Assert.Single(_probe.SatelliteRepository.Satellites).DisplayName);
    }

    [Fact]
    public async Task ThrowsSatelliteValidationExceptionAndStoresNothing_WhenTheSatelliteIsInvalid()
    {
        _probe.SatelliteValidator.ValidationErrors = ["Invalid name."];

        SatelliteValidationException exception = await Assert.ThrowsAsync<SatelliteValidationException>(
            () => _probe.SatelliteManager.CreateSatelliteAsync("Keller", null, TestContext.Current.CancellationToken));

        Assert.Equal("Keller", exception.SatelliteName);
        Assert.Equal(["Invalid name."], exception.ValidationErrors);
        Assert.Empty(_probe.SatelliteRepository.Satellites);
    }

    [Fact]
    public async Task ThrowsDuplicateSatelliteException_WhenTheNameIsAlreadyUsed()
    {
        _probe.AddSatellite("keller");

        await Assert.ThrowsAsync<DuplicateSatelliteException>(
            () => _probe.SatelliteManager.CreateSatelliteAsync("keller", null, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LogsTheCreationWithTheNameButNotTheToken()
    {
        await _probe.SatelliteManager.CreateSatelliteAsync("keller", null, TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Satellite created.", logEntry.Message);
        Assert.Equal("keller", logEntry.ContextData["SatelliteName"]);
        Assert.DoesNotContain(logEntry.ContextData.Values, value => value.Contains("generated-token"));
    }
}
