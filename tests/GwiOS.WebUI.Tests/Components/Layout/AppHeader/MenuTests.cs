using AngleSharp.Dom;
using AppHeaderUnderTest = GwiOS.WebUI.Components.Layout.AppHeader;

namespace GwiOS.WebUI.Tests.Components.Layout.AppHeader;

/// <summary>
/// Covers the navigation of <c>AppHeader</c> and its menu for narrow screens.
/// </summary>
public sealed class MenuTests : BunitContext
{
    [Fact]
    public void LinksToTheModulesInTheDesktopNavigation()
    {
        IRenderedComponent<AppHeaderUnderTest> header = Render<AppHeaderUnderTest>();

        IReadOnlyList<IElement> links = header.FindAll(".gwios-nav-desktop a");
        Assert.Equal(["ToDos", "Admin"], links.Select(link => link.TextContent));
        Assert.Equal(["todos", "admin"], links.Select(link => link.GetAttribute("href")));
    }

    [Fact]
    public void KeepsTheMenuClosed_Initially()
    {
        IRenderedComponent<AppHeaderUnderTest> header = Render<AppHeaderUnderTest>();

        Assert.Empty(header.FindAll(".gwios-nav-mobile"));
        Assert.Equal("false", header.Find(".gwios-burger").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void OpensTheMenu_WhenTheBurgerIsClicked()
    {
        IRenderedComponent<AppHeaderUnderTest> header = Render<AppHeaderUnderTest>();

        header.Find(".gwios-burger").Click();

        Assert.Equal(["ToDos", "Admin"], header.FindAll(".gwios-nav-mobile a").Select(link => link.TextContent));
        Assert.Equal("true", header.Find(".gwios-burger").GetAttribute("aria-expanded"));
        Assert.True(header.Find(".gwios-burger").ClassList.Contains("gwios-burger--open"));
    }

    [Fact]
    public void ClosesTheMenu_WhenTheBurgerIsClickedAgain()
    {
        IRenderedComponent<AppHeaderUnderTest> header = Render<AppHeaderUnderTest>();

        header.Find(".gwios-burger").Click();
        header.Find(".gwios-burger").Click();

        Assert.Empty(header.FindAll(".gwios-nav-mobile"));
    }

    [Fact]
    public void ClosesTheMenu_WhenAnEntryIsChosen()
    {
        IRenderedComponent<AppHeaderUnderTest> header = Render<AppHeaderUnderTest>();
        header.Find(".gwios-burger").Click();

        header.Find(".gwios-nav-mobile").Click();

        Assert.Empty(header.FindAll(".gwios-nav-mobile"));
    }
}
