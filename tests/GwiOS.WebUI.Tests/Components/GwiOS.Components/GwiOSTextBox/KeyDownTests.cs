using Microsoft.AspNetCore.Components.Web;
using GwiOSTextBoxUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSTextBox;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSTextBox;

/// <summary>
/// Covers the keyboard shortcuts of <c>GwiOSTextBox</c>.
/// </summary>
public sealed class KeyDownTests : BunitContext
{
    [Fact]
    public void RaisesOnEnter_WhenEnterIsPressed()
    {
        int enterCount = 0;
        IRenderedComponent<GwiOSTextBoxUnderTest> textBox = Render<GwiOSTextBoxUnderTest>(parameters => parameters
            .Add(component => component.OnEnter, () => enterCount++));

        textBox.Find("input").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.Equal(1, enterCount);
    }

    [Fact]
    public void ClearsTheText_WhenEscapeIsPressed()
    {
        string? reportedValue = null;
        IRenderedComponent<GwiOSTextBoxUnderTest> textBox = Render<GwiOSTextBoxUnderTest>(parameters => parameters
            .Add(component => component.Value, "Einkaufen")
            .Add(component => component.ValueChanged, (string value) => reportedValue = value));

        textBox.Find("input").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        Assert.Equal(string.Empty, reportedValue);
        Assert.Equal(string.Empty, textBox.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void IgnoresOtherKeys()
    {
        int enterCount = 0;
        string? reportedValue = null;
        IRenderedComponent<GwiOSTextBoxUnderTest> textBox = Render<GwiOSTextBoxUnderTest>(parameters => parameters
            .Add(component => component.OnEnter, () => enterCount++)
            .Add(component => component.ValueChanged, (string value) => reportedValue = value));

        textBox.Find("input").KeyDown(new KeyboardEventArgs { Key = "a" });

        Assert.Equal(0, enterCount);
        Assert.Null(reportedValue);
    }
}
