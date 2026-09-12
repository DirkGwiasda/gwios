using GwiOSDateBoxUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSDateBox;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSDateBox;

/// <summary>
/// Covers how <c>GwiOSDateBox</c> reacts when the user picks or clears a date.
/// </summary>
public sealed class ChangeTests : BunitContext
{
    [Fact]
    public void ReportsThePickedDate()
    {
        DateOnly? reportedValue = null;
        IRenderedComponent<GwiOSDateBoxUnderTest> dateBox = Render<GwiOSDateBoxUnderTest>(parameters => parameters
            .Add(component => component.ValueChanged, (DateOnly? value) => reportedValue = value));

        dateBox.Find("input").Change("2026-08-05");

        Assert.Equal(new DateOnly(2026, 8, 5), reportedValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("kein Datum")]
    public void ReportsNoDate_WhenTheInputHoldsNoCompleteDate(string htmlDate)
    {
        DateOnly? reportedValue = new DateOnly(2026, 1, 1);
        IRenderedComponent<GwiOSDateBoxUnderTest> dateBox = Render<GwiOSDateBoxUnderTest>(parameters => parameters
            .Add(component => component.Value, new DateOnly(2026, 1, 1))
            .Add(component => component.ValueChanged, (DateOnly? value) => reportedValue = value));

        dateBox.Find("input").Change(htmlDate);

        Assert.Null(reportedValue);
    }
}
