using GwiOSPillGroupUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSPillGroup;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSPillGroup;

/// <summary>
/// Covers how <c>GwiOSPillGroup</c> reacts when the user clicks an item.
/// </summary>
public sealed class SelectTests : BunitContext
{
    [Fact]
    public void ReportsAndMarksTheClickedItem()
    {
        string? reportedItem = null;
        IRenderedComponent<GwiOSPillGroupUnderTest> pillGroup = Render<GwiOSPillGroupUnderTest>(parameters => parameters
            .Add(component => component.Items, ["GwiOS", "Backup"])
            .Add(component => component.SelectedItem, "GwiOS")
            .Add(component => component.SelectedItemChanged, (string item) => reportedItem = item));

        pillGroup.FindAll("button")[1].Click();

        Assert.Equal("Backup", reportedItem);
        Assert.Equal("true", pillGroup.FindAll("button")[1].GetAttribute("aria-pressed"));
    }

    [Fact]
    public void ReportsNothing_WhenTheSelectedItemIsClicked()
    {
        int reportCount = 0;
        IRenderedComponent<GwiOSPillGroupUnderTest> pillGroup = Render<GwiOSPillGroupUnderTest>(parameters => parameters
            .Add(component => component.Items, ["GwiOS", "Backup"])
            .Add(component => component.SelectedItem, "GwiOS")
            .Add(component => component.SelectedItemChanged, (string _) => reportCount++));

        pillGroup.FindAll("button")[0].Click();

        Assert.Equal(0, reportCount);
    }
}
