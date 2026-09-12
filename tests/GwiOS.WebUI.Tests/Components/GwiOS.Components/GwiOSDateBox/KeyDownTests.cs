using Microsoft.AspNetCore.Components.Web;
using GwiOSDateBoxUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSDateBox;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSDateBox;

/// <summary>
/// Covers the keyboard shortcuts of <c>GwiOSDateBox</c>.
/// </summary>
public sealed class KeyDownTests : BunitContext
{
    [Fact]
    public void RaisesOnEnter_WhenEnterIsPressed()
    {
        int enterCount = 0;
        IRenderedComponent<GwiOSDateBoxUnderTest> dateBox = Render<GwiOSDateBoxUnderTest>(parameters => parameters
            .Add(component => component.OnEnter, () => enterCount++));

        dateBox.Find("input").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.Equal(1, enterCount);
    }

    [Fact]
    public void ClearsTheDate_WhenEscapeIsPressed()
    {
        DateOnly? reportedValue = new DateOnly(2026, 1, 1);
        IRenderedComponent<GwiOSDateBoxUnderTest> dateBox = Render<GwiOSDateBoxUnderTest>(parameters => parameters
            .Add(component => component.Value, new DateOnly(2026, 1, 1))
            .Add(component => component.ValueChanged, (DateOnly? value) => reportedValue = value));

        dateBox.Find("input").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        Assert.Null(reportedValue);
        Assert.Equal(string.Empty, dateBox.Find("input").GetAttribute("value"));
    }
}
