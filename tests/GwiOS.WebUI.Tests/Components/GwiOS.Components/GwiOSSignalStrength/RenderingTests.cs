using GwiOSSignalStrengthUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSSignalStrength;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSSignalStrength;

/// <summary>
/// Covers how <c>GwiOSSignalStrength</c> turns a signal strength into bars.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Theory]
    [InlineData(-45, 4)]
    [InlineData(-52, 3)]
    [InlineData(-70, 2)]
    [InlineData(-81, 1)]
    [InlineData(-100, 1)]
    public void LightsUpTheBarsOfTheStrength(int rssiDbm, int expectedBarCount)
    {
        IRenderedComponent<GwiOSSignalStrengthUnderTest> signal = Render<GwiOSSignalStrengthUnderTest>(parameters => parameters
            .Add(component => component.RssiDbm, rssiDbm));

        Assert.Equal(4, signal.FindAll(".gwios-signal-bar").Count);
        Assert.Equal(expectedBarCount, signal.FindAll(".gwios-signal-bar--on").Count);
        Assert.Equal($"Signalstärke {expectedBarCount} von 4", signal.Find(".gwios-signal").GetAttribute("aria-label"));
    }

    [Fact]
    public void LightsUpNoBar_WhenTheStrengthIsNotCurrent()
    {
        IRenderedComponent<GwiOSSignalStrengthUnderTest> signal = Render<GwiOSSignalStrengthUnderTest>(parameters => parameters
            .Add(component => component.RssiDbm, -45)
            .Add(component => component.IsActive, false));

        Assert.Empty(signal.FindAll(".gwios-signal-bar--on"));
    }
}
