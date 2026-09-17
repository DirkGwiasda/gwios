using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;
using PersonAdministrationUnderTest = GwiOS.WebUI.Components.Pages.Admin.PersonAdministration;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.PersonAdministration;

/// <summary>
/// Covers closing the edit dialog of a person on <c>PersonAdministration</c> without saving.
/// </summary>
public sealed class CloseEditorTests : PersonAdministrationProbe
{
    [Fact]
    public void RemovesTheDialogWithoutChangingThePerson_WhenCancelIsClicked()
    {
        PersonManager.Add(new Person { Name = "Anna", ShortName = "A" });
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPage();
        OpenEditor(page, "Anna");
        EnterIntoEditor(page, "person-edit-name", "Anna Beispiel");

        page.FindAll(".gwios-dialog-actions button").Single(button => button.TextContent.Trim() == "Abbrechen").Click();

        Assert.Empty(page.FindAll("[role='dialog']"));
        Assert.Equal("Anna", Assert.Single(PersonManager.Persons).Name);
    }

    [Fact]
    public void RemovesTheDialog_WhenEscapeIsPressed()
    {
        PersonManager.Add(new Person { Name = "Anna", ShortName = "A" });
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPage();
        OpenEditor(page, "Anna");

        page.Find("[role='dialog']").KeyDown("Escape");

        Assert.Empty(page.FindAll("[role='dialog']"));
    }
}
