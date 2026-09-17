using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using GwiOSSatelliteCandidateTileUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSSatelliteCandidateTile;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSSatelliteCandidateTile;

/// <summary>
/// Covers how <c>GwiOSSatelliteCandidateTile</c> reports clicks on its buttons.
/// </summary>
public sealed class ClickTests : BunitContext
{
    private int _adoptCount;
    private int _blockCount;

    public ClickTests()
    {
        Services.AddSingleton<TimeProvider>(new TimeProviderFake());
    }

    [Fact]
    public void RaisesOnAdopt_WhenAnlegenIsClicked()
    {
        IRenderedComponent<GwiOSSatelliteCandidateTileUnderTest> tile = RenderTile();

        tile.Find("button[title='„garage“ mit seinem Token anlegen']").Click();

        Assert.Equal(1, _adoptCount);
    }

    [Fact]
    public void RaisesOnBlockOnlyAfterTheSecondClick()
    {
        IRenderedComponent<GwiOSSatelliteCandidateTileUnderTest> tile = RenderTile();

        tile.Find("button[title='„garage“ blockieren']").Click();
        Assert.Equal(0, _blockCount);
        tile.Find(".gwios-button--danger").Click();

        Assert.Equal(1, _blockCount);
    }

    private IRenderedComponent<GwiOSSatelliteCandidateTileUnderTest> RenderTile()
        => Render<GwiOSSatelliteCandidateTileUnderTest>(parameters => parameters
            .Add(component => component.Candidate, new TestSatelliteEntries().CreateCandidate("garage"))
            .Add(component => component.Now, TestSatelliteEntries.Now)
            .Add(component => component.OnAdopt, () => _adoptCount++)
            .Add(component => component.OnBlock, () => _blockCount++));
}
