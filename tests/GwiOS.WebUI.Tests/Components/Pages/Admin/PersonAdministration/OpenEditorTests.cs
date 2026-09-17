using AngleSharp.Dom;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;
using PersonAdministrationUnderTest = GwiOS.WebUI.Components.Pages.Admin.PersonAdministration;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.PersonAdministration;

/// <summary>
/// Covers opening the edit dialog of a person on <c>PersonAdministration</c>.
/// </summary>
public sealed class OpenEditorTests : PersonAdministrationProbe
{
    [Fact]
    public void ShowsTheDialogWithTheValuesOfThePerson()
    {
        PersonManager.Add(new Person { Name = "Anna Beispiel", ShortName = "Anna", IdentityUserId = "user-1" });
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPage();

        OpenEditor(page, "Anna Beispiel");

        IElement dialog = page.Find("[role='dialog']");
        Assert.Equal("Person bearbeiten", dialog.QuerySelector(".gwios-dialog-title")!.TextContent);
        Assert.Equal("Anna Beispiel", page.Find("#person-edit-name").GetAttribute("value"));
        Assert.Equal("Anna", page.Find("#person-edit-short-name").GetAttribute("value"));
        Assert.Equal("user-1", page.Find("#person-edit-identity-user-id").GetAttribute("value"));
    }

    [Fact]
    public void ShowsAnEmptyUserAccount_WhenThePersonHasNone()
    {
        PersonManager.Add(new Person { Name = "Anna", ShortName = "A" });
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPage();

        OpenEditor(page, "Anna");

        Assert.Equal(string.Empty, page.Find("#person-edit-identity-user-id").GetAttribute("value"));
    }

    [Fact]
    public void ShowsNoErrorsOfAnEarlierAttempt()
    {
        PersonManager.Add(new Person { Name = "Anna", ShortName = "A" });
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPage();
        OpenEditor(page, "Anna");
        EnterIntoEditor(page, "person-edit-name", "");
        FindSaveButton(page).Click();
        page.Find(".gwios-dialog-close").Click();

        OpenEditor(page, "Anna");

        Assert.Empty(page.FindAll(".gwios-form-field-error"));
    }
}
