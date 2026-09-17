using GwiOSReadOnlyFieldUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSReadOnlyField;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSReadOnlyField;

/// <summary>
/// Covers how <c>GwiOSReadOnlyField</c> shows a value that cannot be changed.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Fact]
    public void ShowsLabelValueMarkerAndHint()
    {
        IRenderedComponent<GwiOSReadOnlyFieldUnderTest> field = Render<GwiOSReadOnlyFieldUnderTest>(parameters => parameters
            .Add(component => component.Label, "Name")
            .Add(component => component.Value, "keller")
            .Add(component => component.Marker, "fest")
            .Add(component => component.Hint, "Steht auf dem Board."));

        Assert.Equal("Name", field.Find(".gwios-read-only-field-label").TextContent);
        Assert.Equal("keller", field.Find(".gwios-read-only-field-value").TextContent);
        Assert.Equal("fest", field.Find(".gwios-read-only-field-marker").TextContent);
        Assert.Equal("Steht auf dem Board.", field.Find(".gwios-read-only-field-hint").TextContent);
        Assert.Empty(field.FindAll(".gwios-read-only-field-hint--warning"));
    }

    [Fact]
    public void ShowsNeitherMarkerNorHint_WhenNoneAreGiven()
    {
        IRenderedComponent<GwiOSReadOnlyFieldUnderTest> field = Render<GwiOSReadOnlyFieldUnderTest>(parameters => parameters
            .Add(component => component.Label, "Name")
            .Add(component => component.Value, "keller"));

        Assert.Empty(field.FindAll(".gwios-read-only-field-marker"));
        Assert.Empty(field.FindAll(".gwios-read-only-field-hint"));
    }

    [Fact]
    public void MarksAWarningHint()
    {
        IRenderedComponent<GwiOSReadOnlyFieldUnderTest> field = Render<GwiOSReadOnlyFieldUnderTest>(parameters => parameters
            .Add(component => component.Label, "Name")
            .Add(component => component.Value, "keller")
            .Add(component => component.Hint, "Vorsicht.")
            .Add(component => component.IsHintWarning, true));

        Assert.Single(field.FindAll(".gwios-read-only-field-hint--warning"));
    }
}
