using AngleSharp.Dom;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using PersonAdministrationUnderTest = GwiOS.WebUI.Components.Pages.Admin.PersonAdministration;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.PersonAdministration;

/// <summary>
/// Covers saving the changes of the edit dialog on <c>PersonAdministration</c>.
/// </summary>
public sealed class SavePersonAsyncTests : PersonAdministrationProbe
{
    private const string SaveFailureMessage = "Person konnte nicht gespeichert werden – bitte Eingaben prüfen";

    private readonly Person _anna = new() { Name = "Anna", ShortName = "A" };

    [Fact]
    public void StoresTheChangedValuesTrimmed()
    {
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPageEditingAnna();
        EnterIntoEditor(page, "person-edit-name", " Anna Beispiel ");
        EnterIntoEditor(page, "person-edit-short-name", " Anni ");
        EnterIntoEditor(page, "person-edit-identity-user-id", " user-7 ");

        FindSaveButton(page).Click();

        Person person = Assert.Single(PersonManager.Persons);
        Assert.Equal(_anna.Id, person.Id);
        Assert.Equal("Anna Beispiel", person.Name);
        Assert.Equal("Anni", person.ShortName);
        Assert.Equal("user-7", person.IdentityUserId);
    }

    [Fact]
    public void RemovesTheUserAccount_WhenTheFieldIsCleared()
    {
        _anna.IdentityUserId = "user-1";
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPageEditingAnna();
        EnterIntoEditor(page, "person-edit-identity-user-id", "  ");

        FindSaveButton(page).Click();

        Assert.Null(Assert.Single(PersonManager.Persons).IdentityUserId);
    }

    [Fact]
    public void ClosesTheDialogAndShowsTheChangedPerson()
    {
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPageEditingAnna();
        EnterIntoEditor(page, "person-edit-name", "Anna Beispiel");

        FindSaveButton(page).Click();

        Assert.Empty(page.FindAll("[role='dialog']"));
        Assert.Equal("Anna Beispiel", page.Find(".gwios-person-name").TextContent);
    }

    [Fact]
    public void PublishesASuccessStatusMessage()
    {
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPageEditingAnna();
        EnterIntoEditor(page, "person-edit-name", "Anna Beispiel");

        FindSaveButton(page).Click();

        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Success, message.Level);
        Assert.Equal("Person „Anna Beispiel“ gespeichert", message.Text);
    }

    [Fact]
    public void Saves_WhenEnterIsPressedInAField()
    {
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPageEditingAnna();
        EnterIntoEditor(page, "person-edit-short-name", "Anni");

        page.Find("#person-edit-short-name").KeyDown("Enter");

        Assert.Equal("Anni", Assert.Single(PersonManager.Persons).ShortName);
    }

    [Fact]
    public void ShowsAnErrorAtEachEmptyRequiredFieldAndKeepsThePerson()
    {
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPageEditingAnna();
        EnterIntoEditor(page, "person-edit-name", " ");
        EnterIntoEditor(page, "person-edit-short-name", "");

        FindSaveButton(page).Click();

        Assert.Equal(
            ["Der Name ist ein Pflichtfeld.", "Der Kurzname ist ein Pflichtfeld."],
            page.FindAll(".gwios-form-field-error").Select(error => error.TextContent));
        Assert.Single(page.FindAll("[role='dialog']"));
        Assert.Equal("Anna", Assert.Single(PersonManager.Persons).Name);
    }

    [Fact]
    public void PublishesAnErrorStatusMessage_WhenARequiredFieldIsEmpty()
    {
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPageEditingAnna();
        EnterIntoEditor(page, "person-edit-name", "");

        FindSaveButton(page).Click();

        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Error, message.Level);
        Assert.Equal(SaveFailureMessage, message.Text);
    }

    [Theory]
    [InlineData("person-edit-name", "Bernd", "Dieser Name ist bereits vergeben")]
    [InlineData("person-edit-short-name", "B", "Dieser Kurzname ist bereits vergeben")]
    [InlineData("person-edit-identity-user-id", "user-2", "Dieses Benutzerkonto ist bereits verknüpft")]
    public void ShowsAnErrorAtTheFieldWhoseValueIsAlreadyUsed(string inputId, string usedValue, string expectedError)
    {
        PersonManager.Add(new Person { Name = "Bernd", ShortName = "B", IdentityUserId = "user-2" });
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPageEditingAnna();
        EnterIntoEditor(page, inputId, usedValue);

        FindSaveButton(page).Click();

        IElement error = page.Find(".gwios-form-field-error");
        Assert.Equal(expectedError, error.TextContent);
        Assert.Equal(inputId, error.ParentElement!.QuerySelector("input")!.Id);
        Assert.Equal(SaveFailureMessage, Assert.Single(StatusMessages.Messages).Text);
    }

    [Fact]
    public void RemovesTheErrorOfAField_WhenTheFieldIsChanged()
    {
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPageEditingAnna();
        EnterIntoEditor(page, "person-edit-name", "");
        FindSaveButton(page).Click();

        EnterIntoEditor(page, "person-edit-name", "Anna Beispiel");

        Assert.Empty(page.FindAll(".gwios-form-field-error"));
    }

    [Fact]
    public void ShowsThatItSavesAndCannotBeClosed_WhileSaving()
    {
        TaskCompletionSource updateRelease = new();
        PersonManager.UpdateReleased = updateRelease.Task;
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPageEditingAnna();

        FindSaveButton(page).Click();

        IElement saveButton = FindSaveButton(page);
        Assert.Equal("Speichert …", saveButton.TextContent.Trim());
        Assert.True(saveButton.HasAttribute("disabled"));
        Assert.True(page.Find(".gwios-dialog-close").HasAttribute("disabled"));
        page.Find("[role='dialog']").KeyDown("Escape");
        Assert.Single(page.FindAll("[role='dialog']"));

        updateRelease.SetResult();

        page.WaitForAssertion(() => Assert.Empty(page.FindAll("[role='dialog']")));
    }

    [Fact]
    public void ClosesTheDialogAndPublishesAnError_WhenThePersonWasDeletedMeanwhile()
    {
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPageEditingAnna();
        PersonManager.Remove(_anna);

        FindSaveButton(page).Click();

        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Error, message.Level);
        Assert.Equal("Person „Anna“ gibt es nicht mehr", message.Text);
        Assert.Empty(page.FindAll("[role='dialog']"));
        Assert.Empty(page.FindAll(".gwios-person-tile"));
    }

    private IRenderedComponent<PersonAdministrationUnderTest> RenderPageEditingAnna()
    {
        PersonManager.Add(_anna);
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPage();
        OpenEditor(page, _anna.Name);
        return page;
    }
}
