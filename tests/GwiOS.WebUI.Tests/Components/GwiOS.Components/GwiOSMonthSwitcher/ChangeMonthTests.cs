using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using GwiOSMonthSwitcherUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSMonthSwitcher;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSMonthSwitcher;

/// <summary>
/// Covers which month <c>GwiOSMonthSwitcher</c> reports when the user clicks one of its buttons.
/// </summary>
public sealed class ChangeMonthTests : BunitContext
{
    private DateOnly? _reportedMonth;

    public ChangeMonthTests()
        => Services.AddSingleton<TimeProvider>(new TimeProviderFake());

    [Fact]
    public void ReportsThePreviousMonth_AcrossTheTurnOfTheYear()
    {
        IRenderedComponent<GwiOSMonthSwitcherUnderTest> switcher = RenderSwitcher(new DateOnly(2026, 1, 1));

        switcher.Find("button[aria-label='Voriger Monat']").Click();

        Assert.Equal(new DateOnly(2025, 12, 1), _reportedMonth);
    }

    [Fact]
    public void ReportsTheNextMonth()
    {
        IRenderedComponent<GwiOSMonthSwitcherUnderTest> switcher = RenderSwitcher(new DateOnly(2026, 3, 1));

        switcher.Find("button[aria-label='Nächster Monat']").Click();

        Assert.Equal(new DateOnly(2026, 4, 1), _reportedMonth);
    }

    [Fact]
    public void ReportsTheCurrentMonth_WhenTodayIsClicked()
    {
        IRenderedComponent<GwiOSMonthSwitcherUnderTest> switcher = RenderSwitcher(new DateOnly(2025, 3, 1));

        switcher.Find(".gwios-toggle-button").Click();

        Assert.Equal(new DateOnly(2026, 7, 1), _reportedMonth);
    }

    private IRenderedComponent<GwiOSMonthSwitcherUnderTest> RenderSwitcher(DateOnly month)
        => Render<GwiOSMonthSwitcherUnderTest>(parameters => parameters
            .Add(component => component.Month, month)
            .Add(component => component.MonthChanged, (DateOnly reportedMonth) => _reportedMonth = reportedMonth));
}
