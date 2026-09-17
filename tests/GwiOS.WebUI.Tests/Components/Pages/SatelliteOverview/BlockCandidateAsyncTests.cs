using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using SatelliteOverviewUnderTest = GwiOS.WebUI.Components.Pages.SatelliteOverview;

namespace GwiOS.WebUI.Tests.Components.Pages.SatelliteOverview;

/// <summary>
/// Covers blocking an unknown satellite on <c>SatelliteOverview</c>.
/// </summary>
public sealed class BlockCandidateAsyncTests : SatelliteOverviewProbe
{
    public BlockCandidateAsyncTests()
    {
        OverviewWorkflow.Candidates.Add(Entries.CreateCandidate("fremd"));
    }

    [Fact]
    public void BlocksTheCandidateAfterConfirmation()
    {
        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();

        Block(page);

        Assert.Equal(["fremd"], BlockingWorkflow.BlockedNames);
        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Warning, message.Level);
        Assert.Equal("Satellit „fremd“ blockiert", message.Text);
        Assert.Equal(2, OverviewWorkflow.CallCount);
    }

    [Fact]
    public void PublishesAWarning_WhenTheSatelliteExistsAlready()
    {
        BlockingWorkflow.ExceptionToThrow = new DuplicateSatelliteException("fremd", new InvalidOperationException());
        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();

        Block(page);

        Assert.Equal("Satellit „fremd“ gibt es bereits", Assert.Single(StatusMessages.Messages).Text);
    }

    private static void Block(IRenderedComponent<SatelliteOverviewUnderTest> page)
    {
        page.Find("button[title='„fremd“ blockieren']").Click();
        page.Find(".gwios-satellite-candidate-actions .gwios-button--danger").Click();
    }
}
