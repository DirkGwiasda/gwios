using GwiOSTrendArrowUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSTrendArrow;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSTrendArrow;

/// <summary>
/// Covers how <c>GwiOSTrendArrow</c> draws an arrow for a tendency.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Theory]
    [InlineData(0, "rotate(0 12 12)")]
    [InlineData(-45, "rotate(-45 12 12)")]
    [InlineData(90, "rotate(90 12 12)")]
    public void TurnsTheArrowAroundTheMiddle(double rotationDegrees, string expectedTransform)
    {
        IRenderedComponent<GwiOSTrendArrowUnderTest> arrow = Render<GwiOSTrendArrowUnderTest>(parameters => parameters
            .Add(component => component.RotationDegrees, rotationDegrees)
            .Add(component => component.AriaLabel, "steigend"));

        Assert.Equal(expectedTransform, arrow.Find("g").GetAttribute("transform"));
    }

    [Fact]
    public void DrawsALineWithAnArrowHead()
    {
        IRenderedComponent<GwiOSTrendArrowUnderTest> arrow = Render<GwiOSTrendArrowUnderTest>(parameters => parameters
            .Add(component => component.RotationDegrees, 0)
            .Add(component => component.AriaLabel, "gleichbleibend"));

        Assert.Single(arrow.FindAll("line"));
        Assert.Equal("13,6 20,12 13,18", arrow.Find("polyline").GetAttribute("points"));
    }

    [Fact]
    public void NamesTheMeaningForMouseAndScreenReader()
    {
        IRenderedComponent<GwiOSTrendArrowUnderTest> arrow = Render<GwiOSTrendArrowUnderTest>(parameters => parameters
            .Add(component => component.RotationDegrees, 0)
            .Add(component => component.AriaLabel, "gleichbleibend: 0,0 hPa in 3 h"));

        Assert.Equal("gleichbleibend: 0,0 hPa in 3 h", arrow.Find("svg").GetAttribute("aria-label"));
        Assert.Equal("gleichbleibend: 0,0 hPa in 3 h", arrow.Find("title").TextContent);
    }
}
