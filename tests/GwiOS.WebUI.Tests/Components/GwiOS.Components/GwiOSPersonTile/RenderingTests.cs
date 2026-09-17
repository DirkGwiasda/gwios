using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;
using GwiOSPersonTileUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSPersonTile;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSPersonTile;

/// <summary>
/// Covers how <c>GwiOSPersonTile</c> renders a person and reports clicks on its buttons.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Fact]
    public void ShowsTheNameTheShortNameAndTheFirstTwoCharactersOfTheShortNameInUppercase()
    {
        IRenderedComponent<GwiOSPersonTileUnderTest> tile =
            RenderTile(new Person { Name = "Anna Beispiel", ShortName = "anni" });

        Assert.Equal("Anna Beispiel", tile.Find(".gwios-person-name").TextContent);
        Assert.Equal("Anna Beispiel", tile.Find(".gwios-person-name").GetAttribute("title"));
        Assert.Equal("anni", tile.Find(".gwios-person-short-name").TextContent);
        Assert.Equal("AN", tile.Find(".gwios-person-avatar").TextContent);
    }

    [Fact]
    public void ShowsTheWholeShortNameAsInitials_WhenItHasOnlyOneCharacter()
    {
        IRenderedComponent<GwiOSPersonTileUnderTest> tile = RenderTile(new Person { Name = "Anna", ShortName = "a" });

        Assert.Equal("A", tile.Find(".gwios-person-avatar").TextContent);
    }

    [Fact]
    public void ShowsTheInitialsOfTheName_WhenTheShortNameIsEmpty()
    {
        IRenderedComponent<GwiOSPersonTileUnderTest> tile = RenderTile(new Person { Name = "Bernd", ShortName = "" });

        Assert.Equal("BE", tile.Find(".gwios-person-avatar").TextContent);
    }

    [Fact]
    public void ShowsAQuestionMarkAsInitials_WhenNameAndShortNameAreEmpty()
    {
        IRenderedComponent<GwiOSPersonTileUnderTest> tile = RenderTile(new Person { Name = "", ShortName = " " });

        Assert.Equal("?", tile.Find(".gwios-person-avatar").TextContent);
    }

    [Fact]
    public void MarksAPersonWithoutUserAccount()
    {
        IRenderedComponent<GwiOSPersonTileUnderTest> tile = RenderTile(new Person { Name = "Anna", ShortName = "A" });

        Assert.Equal("kein Konto", tile.Find(".gwios-badge--muted").TextContent);
    }

    [Fact]
    public void MarksAPersonWithUserAccount()
    {
        IRenderedComponent<GwiOSPersonTileUnderTest> tile =
            RenderTile(new Person { Name = "Anna", ShortName = "A", IdentityUserId = "user-1" });

        Assert.Equal("Konto", tile.Find(".gwios-badge--success").TextContent);
    }

    [Fact]
    public void RaisesOnEdit_WhenTheEditButtonIsClicked()
    {
        int editCount = 0;
        IRenderedComponent<GwiOSPersonTileUnderTest> tile = Render<GwiOSPersonTileUnderTest>(parameters => parameters
            .Add(component => component.Person, new Person { Name = "Anna", ShortName = "A" })
            .Add(component => component.OnEdit, () => editCount++));

        tile.Find("button[title='„Anna“ bearbeiten']").Click();

        Assert.Equal(1, editCount);
    }

    [Fact]
    public void RaisesOnDelete_OnlyAfterTheDeletionIsConfirmed()
    {
        int deleteCount = 0;
        IRenderedComponent<GwiOSPersonTileUnderTest> tile = Render<GwiOSPersonTileUnderTest>(parameters => parameters
            .Add(component => component.Person, new Person { Name = "Anna", ShortName = "A" })
            .Add(component => component.OnDelete, () => deleteCount++));

        tile.Find("button[title='„Anna“ löschen']").Click();
        int deleteCountAfterFirstClick = deleteCount;
        tile.Find("button.gwios-icon-button--danger").Click();

        Assert.Equal(0, deleteCountAfterFirstClick);
        Assert.Equal(1, deleteCount);
    }

    [Fact]
    public void AsksToClickAgain_AfterTheFirstClickOnDelete()
    {
        IRenderedComponent<GwiOSPersonTileUnderTest> tile = RenderTile(new Person { Name = "Anna", ShortName = "A" });

        tile.Find("button[title='„Anna“ löschen']").Click();

        Assert.Equal("Sicher?", tile.Find("button.gwios-icon-button--danger").TextContent.Trim());
        Assert.Equal(
            "Zum Bestätigen erneut klicken",
            tile.Find("button.gwios-icon-button--danger").GetAttribute("title"));
    }

    private IRenderedComponent<GwiOSPersonTileUnderTest> RenderTile(Person person)
        => Render<GwiOSPersonTileUnderTest>(parameters => parameters.Add(component => component.Person, person));
}
