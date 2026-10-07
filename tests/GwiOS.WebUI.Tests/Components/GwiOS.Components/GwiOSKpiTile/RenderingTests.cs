using AngleSharp.Dom;
using GwiOS.WebUI.Components.GwiOS.Components;
using GwiOSKpiTileUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSKpiTile;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSKpiTile;

/// <summary>
/// Covers how <c>GwiOSKpiTile</c> renders its figure, footer and colors.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Fact]
    public void RendersTheLabelAndTheValue()
    {
        IRenderedComponent<GwiOSKpiTileUnderTest> tile = Render<GwiOSKpiTileUnderTest>(parameters => parameters
            .Add(component => component.Label, "Gesamteinnahmen")
            .Add(component => component.Value, "5.050,00 €"));

        Assert.Equal("Gesamteinnahmen", tile.Find(".gwios-kpi-tile-label").TextContent);
        Assert.Equal("5.050,00 €", tile.Find(".gwios-kpi-tile-value").TextContent);
    }

    [Fact]
    public void ColorsTheBorderAndTheValueWithTheAccent()
    {
        IRenderedComponent<GwiOSKpiTileUnderTest> tile = Render<GwiOSKpiTileUnderTest>(parameters => parameters
            .Add(component => component.Label, "Gesamtausgaben")
            .Add(component => component.Value, "2.153,97 €")
            .Add(component => component.Accent, GwiOSKpiAccent.Danger));

        Assert.True(tile.Find(".gwios-kpi-tile").ClassList.Contains("gwios-kpi-tile--danger"));
        Assert.True(tile.Find(".gwios-kpi-tile-value").ClassList.Contains("gwios-kpi-text--danger"));
    }

    [Fact]
    public void ColorsTheValueWithTheValueAccent_WhenItIsSet()
    {
        IRenderedComponent<GwiOSKpiTileUnderTest> tile = Render<GwiOSKpiTileUnderTest>(parameters => parameters
            .Add(component => component.Label, "Verfügbarer Saldo")
            .Add(component => component.Value, "+ 2.896,03 €")
            .Add(component => component.Accent, GwiOSKpiAccent.Primary)
            .Add(component => component.ValueAccent, GwiOSKpiAccent.Success));

        Assert.True(tile.Find(".gwios-kpi-tile").ClassList.Contains("gwios-kpi-tile--primary"));
        Assert.True(tile.Find(".gwios-kpi-tile-value").ClassList.Contains("gwios-kpi-text--success"));
    }

    [Fact]
    public void RendersTheFooterWithAMutedRightText_ByDefault()
    {
        IRenderedComponent<GwiOSKpiTileUnderTest> tile = Render<GwiOSKpiTileUnderTest>(parameters => parameters
            .Add(component => component.Label, "Gesamteinnahmen")
            .Add(component => component.Value, "5.050,00 €")
            .Add(component => component.FooterLeft, "Gebucht 5.050,00 €")
            .Add(component => component.FooterRight, "nichts offen"));

        IReadOnlyList<IElement> footerTexts = tile.FindAll(".gwios-kpi-tile-footer span");
        Assert.Equal(["Gebucht 5.050,00 €", "nichts offen"], footerTexts.Select(text => text.TextContent));
        Assert.True(footerTexts[1].ClassList.Contains("gwios-kpi-text--muted"));
    }

    [Fact]
    public void ColorsTheRightFooterTextWithItsAccent()
    {
        IRenderedComponent<GwiOSKpiTileUnderTest> tile = Render<GwiOSKpiTileUnderTest>(parameters => parameters
            .Add(component => component.Label, "Gesamtausgaben")
            .Add(component => component.Value, "2.153,97 €")
            .Add(component => component.FooterRight, "+ 243,37 € offen")
            .Add(component => component.FooterRightAccent, GwiOSKpiAccent.Warning));

        Assert.True(tile.FindAll(".gwios-kpi-tile-footer span")[1].ClassList.Contains("gwios-kpi-text--warning"));
    }

    [Fact]
    public void RendersNoFooter_WhenNeitherFooterTextIsSet()
    {
        IRenderedComponent<GwiOSKpiTileUnderTest> tile = Render<GwiOSKpiTileUnderTest>(parameters => parameters
            .Add(component => component.Label, "Gesamteinnahmen")
            .Add(component => component.Value, "5.050,00 €"));

        Assert.Empty(tile.FindAll(".gwios-kpi-tile-footer"));
    }
}
