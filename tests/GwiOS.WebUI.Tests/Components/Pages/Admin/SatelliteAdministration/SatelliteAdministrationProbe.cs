using AngleSharp.Dom;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts;
using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using SatelliteAdministrationUnderTest = GwiOS.WebUI.Components.Pages.Admin.SatelliteAdministration;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.SatelliteAdministration;

/// <summary>
/// Base of the <c>SatelliteAdministration</c> tests: provides an in-memory satellite manager, an overview workflow
/// built on it, a status message service and a clock in UTC.
/// </summary>
public abstract class SatelliteAdministrationProbe : BunitContext
{
    protected SatelliteAdministrationProbe()
    {
        OverviewWorkflow = new SatelliteOverviewWorkflowFake(SatelliteManager);
        Services.AddSingleton<ISatelliteManager>(SatelliteManager);
        Services.AddSingleton<ISatelliteOverviewWorkflow>(OverviewWorkflow);
        Services.AddSingleton<IStatusMessageService>(StatusMessages);
        Services.AddSingleton<TimeProvider>(new TimeProviderFake());
    }

    /// <summary>
    /// The satellite manager the page works with.
    /// </summary>
    protected SatelliteManagerFake SatelliteManager { get; } = new();

    /// <summary>
    /// The overview workflow the page reads with.
    /// </summary>
    protected SatelliteOverviewWorkflowFake OverviewWorkflow { get; }

    /// <summary>
    /// The status message service the page publishes to.
    /// </summary>
    protected StatusMessageServiceFake StatusMessages { get; } = new();

    /// <summary>
    /// Stores a satellite with the given name.
    /// </summary>
    protected void AddSatellite(string name, string? displayName = null, bool isLocked = false)
        => SatelliteManager.Add(new Satellite { Name = name, DisplayName = displayName, IsLocked = isLocked, ProtectedToken = "protected" });

    /// <summary>
    /// Renders the page under test.
    /// </summary>
    protected IRenderedComponent<SatelliteAdministrationUnderTest> RenderPage()
        => Render<SatelliteAdministrationUnderTest>();

    /// <summary>
    /// Returns the button of the open dialog with the given text.
    /// </summary>
    protected static IElement FindDialogButton(IRenderedComponent<SatelliteAdministrationUnderTest> page, string text)
        => page.FindAll(".gwios-dialog-actions button").Single(button => button.TextContent.Trim() == text);
}
