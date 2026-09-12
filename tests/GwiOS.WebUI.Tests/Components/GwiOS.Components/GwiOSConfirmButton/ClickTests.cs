using AngleSharp.Dom;
using GwiOSConfirmButtonUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSConfirmButton;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSConfirmButton;

/// <summary>
/// Covers the two-step confirmation of <c>GwiOSConfirmButton</c>.
/// </summary>
public sealed class ClickTests : BunitContext
{
    private int _confirmationCount;

    [Fact]
    public void ShowsTheContentAndTheTitleBeforeTheFirstClick()
    {
        IRenderedComponent<GwiOSConfirmButtonUnderTest> button = RenderConfirmButton();

        IElement element = button.Find("button");
        Assert.Equal("Löschen", element.TextContent.Trim());
        Assert.Equal("Eintrag löschen", element.GetAttribute("title"));
        Assert.Equal("gwios-button", element.GetAttribute("class"));
    }

    [Fact]
    public void AsksForConfirmationWithoutConfirming_OnTheFirstClick()
    {
        IRenderedComponent<GwiOSConfirmButtonUnderTest> button = RenderConfirmButton();

        button.Find("button").Click();

        IElement element = button.Find("button");
        Assert.Equal("Wirklich?", element.TextContent.Trim());
        Assert.Equal("Wirklich?", element.GetAttribute("aria-label"));
        Assert.Equal("gwios-button gwios-button--danger", element.GetAttribute("class"));
        Assert.Equal(0, _confirmationCount);
    }

    [Fact]
    public void ConfirmsAndShowsTheContentAgain_OnTheSecondClick()
    {
        IRenderedComponent<GwiOSConfirmButtonUnderTest> button = RenderConfirmButton();

        button.Find("button").Click();
        button.Find("button").Click();

        Assert.Equal(1, _confirmationCount);
        Assert.Equal("Löschen", button.Find("button").TextContent.Trim());
    }

    [Fact]
    public void RendersADisabledButton_WhenItIsDisabled()
    {
        IRenderedComponent<GwiOSConfirmButtonUnderTest> button = Render<GwiOSConfirmButtonUnderTest>(parameters => parameters
            .Add(component => component.IsDisabled, true));

        Assert.True(button.Find("button").HasAttribute("disabled"));
    }

    private IRenderedComponent<GwiOSConfirmButtonUnderTest> RenderConfirmButton()
        => Render<GwiOSConfirmButtonUnderTest>(parameters => parameters
            .Add(component => component.Title, "Eintrag löschen")
            .Add(component => component.ConfirmText, "Wirklich?")
            .Add(component => component.OnConfirmed, () => _confirmationCount++)
            .AddChildContent("Löschen"));
}
