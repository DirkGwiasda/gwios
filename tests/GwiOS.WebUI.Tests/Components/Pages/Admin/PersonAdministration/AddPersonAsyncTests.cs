using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using PersonAdministrationUnderTest = GwiOS.WebUI.Components.Pages.Admin.PersonAdministration;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.PersonAdministration;

/// <summary>
/// Covers adding a person on <c>PersonAdministration</c>.
/// </summary>
public sealed class AddPersonAsyncTests : PersonAdministrationProbe
{
    [Fact]
    public void CreatesThePersonWithTrimmedValuesWithoutUserAccount()
    {
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPage();

        AddPerson(page, "  Anna Beispiel ", " Anna ");

        Person person = Assert.Single(PersonManager.Persons);
        Assert.Equal("Anna Beispiel", person.Name);
        Assert.Equal("Anna", person.ShortName);
        Assert.Null(person.IdentityUserId);
    }

    [Fact]
    public void ShowsTheNewPersonAndClearsTheInputs()
    {
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPage();

        AddPerson(page, "Anna Beispiel", "Anna");

        Assert.Equal("Anna Beispiel", page.Find(".gwios-person-name").TextContent);
        Assert.All(page.FindAll(".gwios-form-panel input"), input => Assert.Equal(string.Empty, input.GetAttribute("value")));
    }

    [Fact]
    public void PublishesANeutralStatusMessage()
    {
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPage();

        AddPerson(page, "Anna Beispiel", "Anna");

        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Neutral, message.Level);
        Assert.Equal("Person „Anna Beispiel“ hinzugefügt", message.Text);
    }

    [Fact]
    public void PublishesAWarningAndKeepsTheInputs_WhenTheNameIsAlreadyUsed()
    {
        PersonManager.Add(new Person { Name = "Anna", ShortName = "A" });
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPage();

        AddPerson(page, "Anna", "Anni");

        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Warning, message.Level);
        Assert.Equal("Der Name „Anna“ ist bereits vergeben", message.Text);
        Assert.Equal("Anna", page.Find("input[aria-label='Name der neuen Person']").GetAttribute("value"));
        Assert.Single(PersonManager.Persons);
    }

    [Fact]
    public void PublishesAWarning_WhenTheShortNameIsAlreadyUsed()
    {
        PersonManager.Add(new Person { Name = "Anna", ShortName = "A" });
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPage();

        AddPerson(page, "Andreas", "A");

        Assert.Equal("Der Kurzname „A“ ist bereits vergeben", Assert.Single(StatusMessages.Messages).Text);
    }
}
