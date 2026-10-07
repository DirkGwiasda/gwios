using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using GwiOSMonthSwitcherUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSMonthSwitcher;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSMonthSwitcher;

/// <summary>
/// Covers how <c>GwiOSMonthSwitcher</c> shows the month and whether it is the current one.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    public RenderingTests()
        => Services.AddSingleton<TimeProvider>(new TimeProviderFake());

    [Fact]
    public void ShowsTheMonthNameAndYear()
    {
        IRenderedComponent<GwiOSMonthSwitcherUnderTest> switcher = RenderSwitcher(new DateOnly(2026, 3, 1));

        Assert.Equal("März 2026", switcher.Find(".gwios-month-switcher-label").TextContent);
    }

    [Fact]
    public void MarksTheTodayButtonAsSelected_WhenTheCurrentMonthIsShown()
    {
        IRenderedComponent<GwiOSMonthSwitcherUnderTest> switcher = RenderSwitcher(new DateOnly(2026, 7, 1));

        Assert.Equal("true", switcher.Find(".gwios-toggle-button").GetAttribute("aria-pressed"));
        Assert.True(switcher.Find(".gwios-toggle-button").ClassList.Contains("gwios-toggle-button--selected"));
    }

    [Fact]
    public void DoesNotMarkTheTodayButtonAsSelected_WhenAnotherMonthIsShown()
    {
        IRenderedComponent<GwiOSMonthSwitcherUnderTest> switcher = RenderSwitcher(new DateOnly(2025, 7, 1));

        Assert.Equal("false", switcher.Find(".gwios-toggle-button").GetAttribute("aria-pressed"));
        Assert.False(switcher.Find(".gwios-toggle-button").ClassList.Contains("gwios-toggle-button--selected"));
    }

    private IRenderedComponent<GwiOSMonthSwitcherUnderTest> RenderSwitcher(DateOnly month)
        => Render<GwiOSMonthSwitcherUnderTest>(parameters => parameters
            .Add(component => component.Month, month));
}
