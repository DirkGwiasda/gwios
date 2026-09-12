using GwiOS.WebUI.Components.GwiOS.Components;
using GwiOSSectionUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSSection;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSSection;

/// <summary>
/// Covers how <c>GwiOSSection</c> renders its title, count and content.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Fact]
    public void RendersTheTitleAndTheContent()
    {
        IRenderedComponent<GwiOSSectionUnderTest> section = Render<GwiOSSectionUnderTest>(parameters => parameters
            .Add(component => component.Title, "Offen")
            .AddChildContent("<p class=\"content\">Inhalt</p>"));

        Assert.Equal("Offen", section.Find("h2").TextContent);
        Assert.Equal("Inhalt", section.Find("p.content").TextContent);
    }

    [Fact]
    public void RendersTheCount_WhenItIsSet()
    {
        IRenderedComponent<GwiOSSectionUnderTest> section = Render<GwiOSSectionUnderTest>(parameters => parameters
            .Add(component => component.Title, "Offen")
            .Add(component => component.Count, 3));

        Assert.Equal("3", section.Find(".gwios-section-count").TextContent);
    }

    [Fact]
    public void RendersNoCount_WhenItIsNotSet()
    {
        IRenderedComponent<GwiOSSectionUnderTest> section = Render<GwiOSSectionUnderTest>(parameters => parameters
            .Add(component => component.Title, "Offen"));

        Assert.Empty(section.FindAll(".gwios-section-count"));
    }

    [Fact]
    public void RendersTheEmptyTextInsteadOfTheContent_WhenItIsEmpty()
    {
        IRenderedComponent<GwiOSSectionUnderTest> section = Render<GwiOSSectionUnderTest>(parameters => parameters
            .Add(component => component.Title, "Offen")
            .Add(component => component.IsEmpty, true)
            .Add(component => component.EmptyText, "Keine offenen Aufgaben.")
            .AddChildContent("<p class=\"content\">Inhalt</p>"));

        Assert.Equal("Keine offenen Aufgaben.", section.Find(".gwios-empty").TextContent);
        Assert.Empty(section.FindAll("p.content"));
    }

    [Theory]
    [InlineData(GwiOSSectionAccent.Primary, "gwios-section-title--primary")]
    [InlineData(GwiOSSectionAccent.Success, "gwios-section-title--success")]
    public void ColorsTheTitleWithTheAccent(GwiOSSectionAccent accent, string expectedClass)
    {
        IRenderedComponent<GwiOSSectionUnderTest> section = Render<GwiOSSectionUnderTest>(parameters => parameters
            .Add(component => component.Title, "Erledigt")
            .Add(component => component.Accent, accent));

        Assert.True(section.Find("h2").ClassList.Contains(expectedClass));
    }
}
