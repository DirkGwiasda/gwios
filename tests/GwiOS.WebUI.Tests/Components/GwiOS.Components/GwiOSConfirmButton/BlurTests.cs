using GwiOSConfirmButtonUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSConfirmButton;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSConfirmButton;

/// <summary>
/// Covers how <c>GwiOSConfirmButton</c> cancels a pending confirmation when the user leaves it.
/// </summary>
public sealed class BlurTests : BunitContext
{
    [Fact]
    public void CancelsTheConfirmation_WhenTheUserLeavesTheButton()
    {
        int confirmationCount = 0;
        IRenderedComponent<GwiOSConfirmButtonUnderTest> button = Render<GwiOSConfirmButtonUnderTest>(parameters => parameters
            .Add(component => component.OnConfirmed, () => confirmationCount++)
            .AddChildContent("Löschen"));

        button.Find("button").Click();
        button.Find("button").Blur();
        button.Find("button").Click();

        Assert.Equal(0, confirmationCount);
        Assert.Equal("Wirklich löschen?", button.Find("button").TextContent.Trim());
    }
}
