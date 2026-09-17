using Microsoft.JSInterop;
using GwiOSSatelliteTokenDetailsUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSSatelliteTokenDetails;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSSatelliteTokenDetails;

/// <summary>
/// Covers copying the token of <c>GwiOSSatelliteTokenDetails</c> to the clipboard.
/// </summary>
public sealed class CopyTokenAsyncTests : BunitContext
{
    [Fact]
    public void WritesTheTokenToTheClipboardAndConfirmsIt()
    {
        JSInterop.SetupVoid("navigator.clipboard.writeText", "my-token").SetVoidResult();
        IRenderedComponent<GwiOSSatelliteTokenDetailsUnderTest> details = RenderDetails();

        details.Find(".gwios-satellite-token-copy").Click();

        Assert.Equal("✓ Kopiert", details.Find(".gwios-satellite-token-copy").TextContent.Trim());
        Assert.Contains("gwios-button--success", details.Find(".gwios-satellite-token-copy").ClassList);
        Assert.Empty(details.FindAll(".gwios-satellite-token-copy-error"));
    }

    [Fact]
    public void AsksToCopyByHand_WhenTheBrowserRefuses()
    {
        JSInterop.SetupVoid("navigator.clipboard.writeText", "my-token").SetException(new JSException("Denied."));
        IRenderedComponent<GwiOSSatelliteTokenDetailsUnderTest> details = RenderDetails();

        details.Find(".gwios-satellite-token-copy").Click();

        Assert.Equal("Kopieren", details.Find(".gwios-satellite-token-copy").TextContent.Trim());
        Assert.Single(details.FindAll(".gwios-satellite-token-copy-error"));
    }

    private IRenderedComponent<GwiOSSatelliteTokenDetailsUnderTest> RenderDetails()
        => Render<GwiOSSatelliteTokenDetailsUnderTest>(parameters => parameters
            .Add(component => component.SatelliteName, "keller")
            .Add(component => component.Token, "my-token"));
}
