using GwiOSSatelliteTokenDetailsUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSSatelliteTokenDetails;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSSatelliteTokenDetails;

/// <summary>
/// Covers how <c>GwiOSSatelliteTokenDetails</c> shows a new token.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Fact]
    public void ShowsTheTokenWithTheWarningThatItIsShownOnlyOnce()
    {
        IRenderedComponent<GwiOSSatelliteTokenDetailsUnderTest> details = Render<GwiOSSatelliteTokenDetailsUnderTest>(parameters => parameters
            .Add(component => component.SatelliteName, "keller")
            .Add(component => component.Token, "my-token"));

        Assert.Equal("my-token", details.Find(".gwios-satellite-token-value").TextContent);
        Assert.Equal("Dieses Token wird nur dieses eine Mal angezeigt.", details.Find(".gwios-satellite-token-warning-title").TextContent);
        Assert.Equal("keller", details.Find(".gwios-satellite-token-warning strong").TextContent);
        Assert.Equal("Kopieren", details.Find(".gwios-satellite-token-copy").TextContent.Trim());
    }
}
