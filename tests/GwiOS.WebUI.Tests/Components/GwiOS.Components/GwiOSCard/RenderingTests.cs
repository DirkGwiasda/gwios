using GwiOS.WebUI.Components.GwiOS.Components;
using GwiOSCardUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSCard;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSCard;

/// <summary>
/// Covers how <c>GwiOSCard</c> renders its header and content.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Fact]
    public void RendersTheTitleAndTheContent()
    {
        IRenderedComponent<GwiOSCardUnderTest> card = Render<GwiOSCardUnderTest>(parameters => parameters
            .Add(component => component.Title, "Soll / Ist")
            .AddChildContent("<p class=\"content\">Inhalt</p>"));

        Assert.Equal("Soll / Ist", card.Find("h2").TextContent);
        Assert.Equal("Inhalt", card.Find("p.content").TextContent);
    }

    [Fact]
    public void RendersTheHint_WhenItIsSet()
    {
        IRenderedComponent<GwiOSCardUnderTest> card = Render<GwiOSCardUnderTest>(parameters => parameters
            .Add(component => component.Title, "Monatsverlauf")
            .Add(component => component.Hint, "Einnahmen gegen Ausgaben"));

        Assert.Equal("Einnahmen gegen Ausgaben", card.Find(".gwios-card-hint").TextContent);
    }

    [Fact]
    public void RendersNoHint_WhenItIsNotSet()
    {
        IRenderedComponent<GwiOSCardUnderTest> card = Render<GwiOSCardUnderTest>(parameters => parameters
            .Add(component => component.Title, "Monatsverlauf"));

        Assert.Empty(card.FindAll(".gwios-card-hint"));
    }

    [Fact]
    public void RendersTheHeaderContentInTheHeader()
    {
        IRenderedComponent<GwiOSCardUnderTest> card = Render<GwiOSCardUnderTest>(parameters => parameters
            .Add(component => component.Title, "Ausgaben")
            .Add(component => component.HeaderContent, "<button class=\"add\">+ Hauptkategorie</button>"));

        Assert.Equal("+ Hauptkategorie", card.Find("header .gwios-card-header-content button.add").TextContent);
    }

    [Fact]
    public void RendersNoHeaderContent_WhenItIsNotSet()
    {
        IRenderedComponent<GwiOSCardUnderTest> card = Render<GwiOSCardUnderTest>(parameters => parameters
            .Add(component => component.Title, "Ausgaben"));

        Assert.Empty(card.FindAll(".gwios-card-header-content"));
    }

    [Fact]
    public void RendersTheEmptyTextInsteadOfTheContent_WhenItIsEmpty()
    {
        IRenderedComponent<GwiOSCardUnderTest> card = Render<GwiOSCardUnderTest>(parameters => parameters
            .Add(component => component.Title, "Soll / Ist")
            .Add(component => component.IsEmpty, true)
            .Add(component => component.EmptyText, "Keine Ausgaben in diesem Zeitraum.")
            .AddChildContent("<p class=\"content\">Inhalt</p>"));

        Assert.Equal("Keine Ausgaben in diesem Zeitraum.", card.Find(".gwios-empty").TextContent);
        Assert.Empty(card.FindAll("p.content"));
    }

    [Theory]
    [InlineData(GwiOSCardAccent.Primary, "gwios-card-title--primary")]
    [InlineData(GwiOSCardAccent.Success, "gwios-card-title--success")]
    [InlineData(GwiOSCardAccent.Danger, "gwios-card-title--danger")]
    public void ColorsTheTitleWithTheAccent(GwiOSCardAccent accent, string expectedClass)
    {
        IRenderedComponent<GwiOSCardUnderTest> card = Render<GwiOSCardUnderTest>(parameters => parameters
            .Add(component => component.Title, "Ausgaben")
            .Add(component => component.Accent, accent));

        Assert.True(card.Find("h2").ClassList.Contains(expectedClass));
    }

    [Theory]
    [InlineData(GwiOSCardVariant.Padded, "gwios-card--padded")]
    [InlineData(GwiOSCardVariant.Flush, "gwios-card--flush")]
    public void LaysOutTheCardAccordingToTheVariant(GwiOSCardVariant variant, string expectedClass)
    {
        IRenderedComponent<GwiOSCardUnderTest> card = Render<GwiOSCardUnderTest>(parameters => parameters
            .Add(component => component.Title, "Ausgaben")
            .Add(component => component.Variant, variant));

        Assert.True(card.Find("section").ClassList.Contains(expectedClass));
    }
}
