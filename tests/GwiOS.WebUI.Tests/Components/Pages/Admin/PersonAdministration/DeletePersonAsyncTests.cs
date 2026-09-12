using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using PersonAdministrationUnderTest = GwiOS.WebUI.Components.Pages.Admin.PersonAdministration;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.PersonAdministration;

/// <summary>
/// Covers deleting a person on <c>PersonAdministration</c>.
/// </summary>
public sealed class DeletePersonAsyncTests : PersonAdministrationProbe
{
    [Fact]
    public void DeletesOnlyThePersonWhoseDeletionIsConfirmed()
    {
        PersonManager.Add(new Person { Name = "Anna", ShortName = "A" });
        PersonManager.Add(new Person { Name = "Bernd", ShortName = "B" });
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPage();

        page.Find("button[title='„Anna“ löschen']").Click();
        page.Find(".gwios-icon-button--danger").Click();

        Assert.Equal("Bernd", Assert.Single(PersonManager.Persons).Name);
        Assert.Equal(["Bernd"], page.FindAll(".gwios-person-name").Select(name => name.TextContent));
    }

    [Fact]
    public void PublishesANeutralStatusMessage()
    {
        PersonManager.Add(new Person { Name = "Anna", ShortName = "A" });
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPage();

        page.Find("button[title='„Anna“ löschen']").Click();
        page.Find(".gwios-icon-button--danger").Click();

        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Neutral, message.Level);
        Assert.Equal("Person „Anna“ gelöscht", message.Text);
    }
}
