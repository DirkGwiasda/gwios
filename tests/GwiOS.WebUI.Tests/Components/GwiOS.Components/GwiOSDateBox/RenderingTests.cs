using GwiOSDateBoxUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSDateBox;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSDateBox;

/// <summary>
/// Covers how <c>GwiOSDateBox</c> renders its value.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Fact]
    public void RendersADateInputWithTheValueInHtmlFormat()
    {
        IRenderedComponent<GwiOSDateBoxUnderTest> dateBox = Render<GwiOSDateBoxUnderTest>(parameters => parameters
            .Add(component => component.Value, new DateOnly(2026, 8, 5)));

        Assert.Equal("date", dateBox.Find("input").GetAttribute("type"));
        Assert.Equal("2026-08-05", dateBox.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void RendersAnEmptyValue_WhenNoDateIsChosen()
    {
        IRenderedComponent<GwiOSDateBoxUnderTest> dateBox = Render<GwiOSDateBoxUnderTest>();

        Assert.Equal(string.Empty, dateBox.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void UsesTheAriaLabelAsAccessibleName()
    {
        IRenderedComponent<GwiOSDateBoxUnderTest> dateBox = Render<GwiOSDateBoxUnderTest>(parameters => parameters
            .Add(component => component.AriaLabel, "Fällig am"));

        Assert.Equal("Fällig am", dateBox.Find("input").GetAttribute("aria-label"));
    }
}
