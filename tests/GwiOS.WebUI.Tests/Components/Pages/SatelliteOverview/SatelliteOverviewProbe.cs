using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Workflows.MeasurementPolling.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteAdoption.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteBlocking.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using SatelliteOverviewUnderTest = GwiOS.WebUI.Components.Pages.SatelliteOverview;

namespace GwiOS.WebUI.Tests.Components.Pages.SatelliteOverview;

/// <summary>
/// Base of the <c>SatelliteOverview</c> tests: provides fakes of the satellite workflows, a status message service and
/// a clock in UTC that only moves when the test advances it, starting at <see cref="TestSatelliteEntries.Now"/>.
/// </summary>
public abstract class SatelliteOverviewProbe : BunitContext
{
    protected SatelliteOverviewProbe()
    {
        OverviewWorkflow = new SatelliteOverviewWorkflowFake(SatelliteManager);
        Services.AddSingleton<ISatelliteOverviewWorkflow>(OverviewWorkflow);
        Services.AddSingleton<IMeasurementPollingWorkflow>(PollingWorkflow);
        Services.AddSingleton<ISatelliteAdoptionWorkflow>(AdoptionWorkflow);
        Services.AddSingleton<ISatelliteBlockingWorkflow>(BlockingWorkflow);
        Services.AddSingleton<IStatusMessageService>(StatusMessages);
        Services.AddSingleton<TimeProvider>(TimeProvider);
    }

    /// <summary>
    /// The satellites the overview is built from.
    /// </summary>
    protected SatelliteManagerFake SatelliteManager { get; } = new();

    /// <summary>
    /// The overview workflow the page reads with.
    /// </summary>
    protected SatelliteOverviewWorkflowFake OverviewWorkflow { get; }

    /// <summary>
    /// The polling workflow behind "Jetzt abfragen".
    /// </summary>
    protected MeasurementPollingWorkflowFake PollingWorkflow { get; } = new();

    /// <summary>
    /// The adoption workflow behind "Anlegen".
    /// </summary>
    protected SatelliteAdoptionWorkflowFake AdoptionWorkflow { get; } = new();

    /// <summary>
    /// The blocking workflow behind "Blockieren".
    /// </summary>
    protected SatelliteBlockingWorkflowFake BlockingWorkflow { get; } = new();

    /// <summary>
    /// The status message service the page publishes to.
    /// </summary>
    protected StatusMessageServiceFake StatusMessages { get; } = new();

    /// <summary>
    /// The clock of the page.
    /// </summary>
    protected FakeTimeProvider TimeProvider { get; } = new(new DateTimeOffset(TestSatelliteEntries.Now));

    /// <summary>
    /// Creates overview entries and candidates.
    /// </summary>
    protected TestSatelliteEntries Entries { get; } = new();

    /// <summary>
    /// Adds the satellite of the given entry and makes the overview return the entry for it.
    /// </summary>
    protected void AddEntry(SatelliteOverviewEntry entry)
    {
        SatelliteManager.Add(entry.Satellite);
        OverviewWorkflow.Entries[entry.Satellite.Name] = entry;
    }

    /// <summary>
    /// Adds a satellite with the given name, which has never pinged.
    /// </summary>
    protected void AddSatellite(string name)
        => SatelliteManager.Add(new Satellite { Name = name });

    /// <summary>
    /// Renders the page under test.
    /// </summary>
    protected IRenderedComponent<SatelliteOverviewUnderTest> RenderPage()
        => Render<SatelliteOverviewUnderTest>();
}
