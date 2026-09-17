using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using SatelliteOverviewUnderTest = GwiOS.WebUI.Components.Pages.SatelliteOverview;

namespace GwiOS.WebUI.Tests.Components.Pages.SatelliteOverview;

/// <summary>
/// Covers adopting an unknown satellite on <c>SatelliteOverview</c>.
/// </summary>
public sealed class AdoptCandidateAsyncTests : SatelliteOverviewProbe
{
    public AdoptCandidateAsyncTests()
    {
        OverviewWorkflow.Candidates.Add(Entries.CreateCandidate("garage"));
    }

    [Fact]
    public void OpensADialogWithTheFixedName()
    {
        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();

        OpenDialog(page);

        Assert.Equal("Satellit anlegen", page.Find(".gwios-dialog-title").TextContent);
        Assert.Equal("garage", page.Find(".gwios-read-only-field-value").TextContent);
    }

    [Fact]
    public void AdoptsTheCandidateWithTheDisplayNameAndClosesTheDialog()
    {
        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();
        OpenDialog(page);

        page.Find("#satellite-adopt-display-name").Input("Garage");
        FindAdoptButton(page).Click();

        Assert.Equal(("garage", "Garage"), Assert.Single(AdoptionWorkflow.Adoptions));
        Assert.Empty(page.FindAll(".gwios-dialog"));
        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Success, message.Level);
        Assert.Equal("Satellit „garage“ angelegt", message.Text);
        Assert.Equal(2, OverviewWorkflow.CallCount);
    }

    [Fact]
    public void KeepsTheDialogOpenWithAnError_WhenTheDisplayNameIsInvalid()
    {
        AdoptionWorkflow.ExceptionToThrow = new SatelliteValidationException("garage", ["Invalid display name."]);
        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();
        OpenDialog(page);

        FindAdoptButton(page).Click();

        Assert.Equal("Der Anzeigename ist ungültig.", page.Find(".gwios-form-field-error").TextContent);
        Assert.Equal(StatusLevel.Error, Assert.Single(StatusMessages.Messages).Level);
    }

    [Fact]
    public void ClosesTheDialogWithAnError_WhenTheCandidateIsGone()
    {
        AdoptionWorkflow.ExceptionToThrow = new SatelliteCandidateNotFoundException("garage");
        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();
        OpenDialog(page);

        FindAdoptButton(page).Click();

        Assert.Empty(page.FindAll(".gwios-dialog"));
        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Error, message.Level);
        Assert.Equal("Satellit „garage“ meldet sich nicht mehr", message.Text);
    }

    [Fact]
    public void ClosesTheDialogWithAnError_WhenTheTokenChanged()
    {
        AdoptionWorkflow.ExceptionToThrow = new SatelliteCandidateTokenChangedException("garage");
        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();
        OpenDialog(page);

        FindAdoptButton(page).Click();

        Assert.Empty(page.FindAll(".gwios-dialog"));
        Assert.Equal(StatusLevel.Error, Assert.Single(StatusMessages.Messages).Level);
    }

    [Fact]
    public void ClosesTheDialogWithAWarning_WhenTheSatelliteExistsAlready()
    {
        AdoptionWorkflow.ExceptionToThrow = new DuplicateSatelliteException("garage", new InvalidOperationException());
        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();
        OpenDialog(page);

        FindAdoptButton(page).Click();

        Assert.Empty(page.FindAll(".gwios-dialog"));
        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Warning, message.Level);
        Assert.Equal("Satellit „garage“ gibt es bereits", message.Text);
    }

    [Fact]
    public void AdoptsNothing_WhenTheDialogIsCancelled()
    {
        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();
        OpenDialog(page);

        page.FindAll(".gwios-dialog-actions button").Single(button => button.TextContent.Trim() == "Abbrechen").Click();

        Assert.Empty(page.FindAll(".gwios-dialog"));
        Assert.Empty(AdoptionWorkflow.Adoptions);
    }

    private static void OpenDialog(IRenderedComponent<SatelliteOverviewUnderTest> page)
        => page.Find("button[title='„garage“ mit seinem Token anlegen']").Click();

    private static AngleSharp.Dom.IElement FindAdoptButton(IRenderedComponent<SatelliteOverviewUnderTest> page)
        => page.FindAll(".gwios-dialog-actions button").Single(button => button.TextContent.Trim() == "Anlegen");
}
