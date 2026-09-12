using AngleSharp.Dom;
using GwiOSPillGroupUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSPillGroup;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSPillGroup;

/// <summary>
/// Covers how <c>GwiOSPillGroup</c> renders its items and the selection.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Fact]
    public void RendersAButtonPerItemInOrder()
    {
        IRenderedComponent<GwiOSPillGroupUnderTest> pillGroup = Render<GwiOSPillGroupUnderTest>(parameters => parameters
            .Add(component => component.Items, ["GwiOS", "Backup"]));

        Assert.Equal(["GwiOS", "Backup"], pillGroup.FindAll("button").Select(button => button.TextContent.Trim()));
    }

    [Fact]
    public void MarksOnlyTheSelectedItem()
    {
        IRenderedComponent<GwiOSPillGroupUnderTest> pillGroup = Render<GwiOSPillGroupUnderTest>(parameters => parameters
            .Add(component => component.Items, ["GwiOS", "Backup"])
            .Add(component => component.SelectedItem, "Backup"));

        IReadOnlyList<IElement> buttons = pillGroup.FindAll("button");
        Assert.Equal(["false", "true"], buttons.Select(button => button.GetAttribute("aria-pressed")));
        Assert.True(buttons[1].ClassList.Contains("gwios-pill--selected"));
        Assert.False(buttons[0].ClassList.Contains("gwios-pill--selected"));
    }
}
