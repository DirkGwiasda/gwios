using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;
using GwiOSPersonTileUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSPersonTile;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSPersonTile;

/// <summary>
/// Covers how <c>GwiOSPersonTile</c> renders a person.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Fact]
    public void ShowsTheNameTheShortNameAndItsUppercaseInitial()
    {
        IRenderedComponent<GwiOSPersonTileUnderTest> tile =
            RenderTile(new Person { Name = "Anna Beispiel", ShortName = "anni" });

        Assert.Equal("Anna Beispiel", tile.Find(".gwios-person-name").TextContent);
        Assert.Equal("anni", tile.Find(".gwios-person-short-name").TextContent);
        Assert.Equal("A", tile.Find(".gwios-person-avatar").TextContent);
    }

    [Fact]
    public void ShowsAQuestionMarkAsInitial_WhenTheShortNameIsEmpty()
    {
        IRenderedComponent<GwiOSPersonTileUnderTest> tile = RenderTile(new Person { Name = "Anna", ShortName = "" });

        Assert.Equal("?", tile.Find(".gwios-person-avatar").TextContent);
    }

    [Fact]
    public void MarksAPersonWithoutUserAccount()
    {
        IRenderedComponent<GwiOSPersonTileUnderTest> tile = RenderTile(new Person { Name = "Anna", ShortName = "A" });

        Assert.Equal("ohne Konto", tile.Find(".gwios-badge").TextContent);
    }

    [Fact]
    public void MarksAPersonWithUserAccount()
    {
        IRenderedComponent<GwiOSPersonTileUnderTest> tile =
            RenderTile(new Person { Name = "Anna", ShortName = "A", IdentityUserId = "user-1" });

        Assert.Equal("Konto", tile.Find(".gwios-badge").TextContent);
    }

    [Fact]
    public void RaisesOnDelete_OnlyAfterTheDeletionIsConfirmed()
    {
        int deleteCount = 0;
        IRenderedComponent<GwiOSPersonTileUnderTest> tile = Render<GwiOSPersonTileUnderTest>(parameters => parameters
            .Add(component => component.Person, new Person { Name = "Anna", ShortName = "A" })
            .Add(component => component.OnDelete, () => deleteCount++));

        tile.Find("button").Click();
        int deleteCountAfterFirstClick = deleteCount;
        tile.Find("button").Click();

        Assert.Equal(0, deleteCountAfterFirstClick);
        Assert.Equal(1, deleteCount);
    }

    private IRenderedComponent<GwiOSPersonTileUnderTest> RenderTile(Person person)
        => Render<GwiOSPersonTileUnderTest>(parameters => parameters.Add(component => component.Person, person));
}
