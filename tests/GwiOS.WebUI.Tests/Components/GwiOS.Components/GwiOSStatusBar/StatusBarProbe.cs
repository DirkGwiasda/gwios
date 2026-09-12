using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using GwiOSStatusBarUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSStatusBar;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSStatusBar;

/// <summary>
/// Base of the <c>GwiOSStatusBar</c> tests: provides an in-memory status message service and a clock whose local
/// time zone is two hours ahead of UTC.
/// </summary>
public abstract class StatusBarProbe : BunitContext
{
    protected StatusBarProbe()
    {
        TimeProvider.UseLocalOffset(TimeSpan.FromHours(2));
        Services.AddSingleton<IStatusMessageService>(StatusMessages);
        Services.AddSingleton<TimeProvider>(TimeProvider);
    }

    /// <summary>
    /// The status message service the status bar shows.
    /// </summary>
    protected StatusMessageServiceFake StatusMessages { get; } = new();

    /// <summary>
    /// The clock the status bar converts times with.
    /// </summary>
    protected TimeProviderFake TimeProvider { get; } = new();

    /// <summary>
    /// Renders the status bar under test.
    /// </summary>
    protected IRenderedComponent<GwiOSStatusBarUnderTest> RenderStatusBar()
        => Render<GwiOSStatusBarUnderTest>();
}
