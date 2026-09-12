using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;
using PersonAdministrationUnderTest = GwiOS.WebUI.Components.Pages.Admin.PersonAdministration;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.PersonAdministration;

/// <summary>
/// Covers how <c>PersonAdministration</c> shows the stored persons.
/// </summary>
public sealed class RenderingTests : PersonAdministrationProbe
{
    [Fact]
    public void ShowsATilePerPersonOrderedByName()
    {
        PersonManager.Add(new Person { Name = "Bernd", ShortName = "B" });
        PersonManager.Add(new Person { Name = "Anna", ShortName = "A" });

        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPage();

        Assert.Equal(["Anna", "Bernd"], page.FindAll(".gwios-person-name").Select(name => name.TextContent));
    }

    [Fact]
    public void ShowsAnEmptyText_WhenNoPersonsAreStored()
    {
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPage();

        Assert.Equal("Keine Personen vorhanden.", page.Find(".gwios-empty").TextContent);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("Anna", "")]
    [InlineData("", "A")]
    [InlineData("Anna", "   ")]
    public void DisablesTheAddButton_WhileNameOrShortNameIsMissing(string name, string shortName)
    {
        IRenderedComponent<PersonAdministrationUnderTest> page = RenderPage();

        page.Find("input[aria-label='Name der neuen Person']").Input(name);
        page.Find("input[aria-label='Kurzname der neuen Person']").Input(shortName);

        Assert.True(page.Find(".gwios-form-panel button").HasAttribute("disabled"));
    }
}
