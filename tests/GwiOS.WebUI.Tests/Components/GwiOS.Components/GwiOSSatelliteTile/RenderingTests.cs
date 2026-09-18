using AngleSharp.Dom;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts.Models;
using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using GwiOSSatelliteTileUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSSatelliteTile;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSSatelliteTile;

/// <summary>
/// Covers how <c>GwiOSSatelliteTile</c> shows a satellite with its values, using a clock in UTC.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    private readonly TestSatelliteEntries _entries = new();

    public RenderingTests()
    {
        Services.AddSingleton<TimeProvider>(new TimeProviderFake());
    }

    [Fact]
    public void ShowsTheDisplayNameAndTheNameWithTheBoard()
    {
        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(_entries.CreateOnline("kinderzimmer", "Kinderzimmer Mila"));

        Assert.Equal("Kinderzimmer Mila", tile.Find(".gwios-satellite-title").TextContent);
        Assert.Equal("kinderzimmer · ESP32 DevKit C", tile.Find(".gwios-satellite-subtitle").TextContent);
    }

    [Fact]
    public void ShowsTheName_WhenTheSatelliteHasNoDisplayName()
    {
        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(_entries.CreateOnline("keller"));

        Assert.Equal("keller", tile.Find(".gwios-satellite-title").TextContent);
    }

    [Fact]
    public void ShowsTheCurrentValuesOfAnOnlineSatelliteInGermanFormat()
    {
        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(_entries.CreateOnline("keller", temperatures: [22.0, 22.44]));

        Assert.Equal("Online", tile.Find(".gwios-badge--success").TextContent);
        Assert.Equal("22,44°", tile.Find(".gwios-satellite-temperature-value").TextContent);
        Assert.Equal("44 %", tile.Find(".gwios-satellite-gauge--humidity .gwios-satellite-gauge-value").FirstChild!.TextContent.Trim());
        Assert.Equal("(8,8 g/m³)", tile.Find(".gwios-satellite-gauge-extra").TextContent);
        Assert.Equal("1014,2 hPa", tile.Find(".gwios-satellite-gauge--pressure .gwios-satellite-gauge-value").FirstChild!.TextContent.Trim());
        Assert.Equal("-52 dBm · 192.168.0.41", tile.Find(".gwios-satellite-network").TextContent);
        Assert.Equal("vor 4 s", tile.Find(".gwios-satellite-seen").TextContent.Trim());
        Assert.Equal("14.07.2026 10:29:56", tile.Find(".gwios-satellite-stamp time").TextContent);
    }

    [Theory]
    [InlineData(new[] { 22.0, 22.4 }, "▲ +0,4 K / 1 h", "up")]
    [InlineData(new[] { 22.4, 22.1 }, "▼ -0,3 K / 1 h", "down")]
    [InlineData(new[] { 22.0, 22.02 }, "► 0,0 K / 1 h", "flat")]
    public void ShowsTheTrendOfTheLastHour(double[] temperatures, string expectedText, string expectedDirection)
    {
        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(_entries.CreateOnline("keller", temperatures: temperatures));

        IElement trend = tile.Find(".gwios-satellite-trend");
        Assert.Equal(expectedText, trend.TextContent);
        Assert.Contains($"gwios-satellite-trend--{expectedDirection}", trend.ClassList);
    }

    [Fact]
    public void ComparesWithTheOldestValueOfTheLastHourFromTheLongHistory()
    {
        SatelliteOverviewEntry entry = _entries.CreateOnline("keller", temperatures: [23.0]);
        MeasurementHistory temperature = entry.Histories.Single(history => history.Quantity == "temperature");
        DateTime now = TestSatelliteEntries.Now;
        temperature.LongTermSamples.AddRange(
        [
            new MeasurementSample(10.0, now.AddMinutes(-90), now.AddMinutes(-90)),
            new MeasurementSample(21.5, now.AddMinutes(-55), now.AddMinutes(-55)),
            new MeasurementSample(22.0, now.AddMinutes(-5), now.AddMinutes(-5))
        ]);

        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(entry);

        Assert.Equal("▲ +1,5 K / 1 h", tile.Find(".gwios-satellite-trend").TextContent);
    }

    [Theory]
    [InlineData(28.0, "hot")]
    [InlineData(24.0, "warm")]
    [InlineData(17.0, "comfortable")]
    [InlineData(16.9, "cold")]
    public void ColorsTheTemperatureByItsValue(double temperature, string expectedHeat)
    {
        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(_entries.CreateOnline("keller", temperatures: [temperature]));

        Assert.Contains($"gwios-satellite-heat--{expectedHeat}", tile.Find(".gwios-satellite-temperature").ClassList);
    }

    [Fact]
    public void DrawsOneBarPerRecentTemperatureWithHeightsBetweenLowestAndHighest()
    {
        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(_entries.CreateOnline("keller", temperatures: [20.0, 21.0, 22.0]));

        IReadOnlyList<IElement> bars = tile.FindAll(".gwios-satellite-spark-bar");
        Assert.Equal(
            ["height:7px;opacity:0.35", "height:20px;opacity:0.567", "height:32px;opacity:0.783"],
            bars.Select(bar => bar.GetAttribute("style")));
    }

    [Theory]
    [InlineData(23.8, 50.0, "(10,8 g/m³)")]
    [InlineData(0.0, 100.0, "(4,8 g/m³)")]
    [InlineData(-10.0, 80.0, "(1,9 g/m³)")]
    public void AddsTheAbsoluteHumidityDerivedFromTemperatureAndRelativeHumidity(
        double temperature,
        double relativeHumidity,
        string expectedText)
    {
        SatelliteOverviewEntry entry = _entries.CreateOnline("keller", temperatures: [temperature]);
        MeasurementHistory humidity = entry.Histories.Single(history => history.Quantity == "humidity");
        humidity.RecentSamples[0] = humidity.RecentSamples[0] with { Value = relativeHumidity };

        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(entry);

        Assert.Equal(expectedText, tile.Find(".gwios-satellite-gauge-extra").TextContent);
    }

    [Fact]
    public void LeavesOutTheAbsoluteHumidity_WhenThereIsNoTemperature()
    {
        SatelliteOverviewEntry entry = _entries.CreateOnline("keller");
        entry.Histories.RemoveAll(history => history.Quantity == "temperature");

        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(entry);

        Assert.Equal("44 %", tile.Find(".gwios-satellite-gauge--humidity .gwios-satellite-gauge-value").TextContent.Trim());
        Assert.Empty(tile.FindAll(".gwios-satellite-gauge-extra"));
    }

    [Fact]
    public void DrawsEqualBarsInTheMiddle_WhenTheValuesAreEqualAsShown()
    {
        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(_entries.CreateOnline("keller", temperatures: [23.801, 23.799, 23.8]));

        Assert.Equal(
            ["height:20px", "height:20px", "height:20px"],
            tile.FindAll(".gwios-satellite-spark-bar").Select(bar => bar.GetAttribute("style")!.Split(';')[0]));
    }

    [Fact]
    public void DrawsSmallSteps_WhenTheValuesDifferInTheSecondDecimal()
    {
        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(_entries.CreateOnline("keller", temperatures: [23.76, 23.84, 23.8]));

        Assert.Equal(
            ["height:18px", "height:22px", "height:20px"],
            tile.FindAll(".gwios-satellite-spark-bar").Select(bar => bar.GetAttribute("style")!.Split(';')[0]));
    }

    [Fact]
    public void DrawsASmallStep_WhenTheValuesDifferByATenth()
    {
        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(_entries.CreateOnline("keller", temperatures: [23.8, 23.9]));

        Assert.Equal(
            ["height:17px", "height:22px"],
            tile.FindAll(".gwios-satellite-spark-bar").Select(bar => bar.GetAttribute("style")!.Split(';')[0]));
    }

    [Fact]
    public void NamesValueUnitAndLocalMeasurementTimeInTheTooltipOfEveryBar()
    {
        TimeProviderFake timeProvider = new();
        timeProvider.UseLocalOffset(TimeSpan.FromHours(2));
        Services.AddSingleton<TimeProvider>(timeProvider);

        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(_entries.CreateOnline("keller", temperatures: [21.96, 22.44]));

        Assert.Equal(
            ["21,96 °C · 12:29:30", "22,44 °C · 12:30:00"],
            tile.FindAll(".gwios-satellite-spark-bar").Select(bar => bar.GetAttribute("title")));
    }

    [Fact]
    public void ShowsPlaceholdersAndTheAgeOfTheLastPing_WhenTheSatelliteIsOffline()
    {
        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(_entries.CreateOffline("garage", minutesSincePing: 6));

        Assert.Contains("gwios-satellite-tile--offline", tile.Find(".gwios-satellite-tile").ClassList);
        Assert.Equal("Offline", tile.Find(".gwios-badge--danger").TextContent);
        Assert.Equal("––,––°", tile.Find(".gwios-satellite-temperature-value").TextContent);
        Assert.Equal("keine Daten", tile.Find(".gwios-satellite-trend").TextContent);
        Assert.Equal("–– %", tile.Find(".gwios-satellite-gauge--humidity .gwios-satellite-gauge-value").TextContent.Trim());
        Assert.Empty(tile.FindAll(".gwios-satellite-gauge-extra"));
        Assert.Equal("–––– hPa", tile.Find(".gwios-satellite-gauge--pressure .gwios-satellite-gauge-value").FirstChild!.TextContent.Trim());
        Assert.Equal("zuletzt vor 6 min", tile.Find(".gwios-satellite-seen").TextContent.Trim());
        Assert.Empty(tile.FindAll(".gwios-signal-bar--on"));
    }

    [Fact]
    public void SaysThatThereAreNoDataYet_WhenTheSatelliteHasNeverPinged()
    {
        SatelliteOverviewEntry entry = new() { Satellite = new Satellite { Name = "neu" }, Histories = [] };

        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(entry);

        Assert.Equal("neu", tile.Find(".gwios-satellite-subtitle").TextContent);
        Assert.Equal("noch keine Daten", tile.Find(".gwios-satellite-stamp").TextContent.Trim());
        Assert.Equal("noch nie gesehen", tile.Find(".gwios-satellite-seen").TextContent);
        Assert.Empty(tile.FindAll(".gwios-satellite-spark-bar"));
    }

    [Fact]
    public void SetsTheWidthOfTheHumidityAndPressureBars()
    {
        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(_entries.CreateOnline("keller"));

        Assert.Equal("width:44.4%", tile.Find(".gwios-satellite-gauge--humidity .gwios-satellite-gauge-fill").GetAttribute("style"));
        Assert.Equal("width:56.8%", tile.Find(".gwios-satellite-gauge--pressure .gwios-satellite-gauge-fill").GetAttribute("style"));
    }

    [Fact]
    public void PutsTheMarkOfTheLocalAveragePressureOnTheScaleFrom954To1060()
    {
        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(_entries.CreateOnline("keller"));

        Assert.Equal("left:39.6%", tile.Find(".gwios-satellite-pressure-mark").GetAttribute("style"));
        Assert.Equal(
            "Skala 954 bis 1060 hPa, Strich beim örtlichen Mittelwert 996 hPa",
            tile.Find(".gwios-satellite-gauge--pressure .gwios-satellite-gauge-track").GetAttribute("title"));
    }

    [Fact]
    public void DrawsThePressureHistoryFromBothHistoriesWithItsSpanAsTooltip()
    {
        SatelliteOverviewEntry entry = _entries.CreateOnline("keller");
        AddPressureHistory(entry, [(-180, 1002.0), (-120, 1001.0), (-60, 1000.0)]);

        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(entry);

        Assert.Equal("0,19.55 33.33,21.03 66.67,22.5 100,1.5", tile.Find(".gwios-satellite-pressure-history polyline").GetAttribute("points"));
        Assert.Equal(
            "Verlauf von 1002,0 bis 1014,2 hPa über 3,0 h",
            tile.Find(".gwios-satellite-pressure-history svg title").TextContent);
    }

    [Fact]
    public void KeepsThePressureHistoryInTheHintOfThePressureValue()
    {
        SatelliteOverviewEntry entry = _entries.CreateOnline("keller");
        AddPressureHistory(entry, [(-60, 1000.0)]);

        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(entry);

        AngleSharp.Dom.IElement history = tile.Find(".gwios-satellite-pressure-value .gwios-satellite-pressure-history");
        Assert.Equal("tooltip", history.GetAttribute("role"));
        Assert.Single(history.QuerySelectorAll("svg.gwios-spark-line"));
        Assert.Equal("0", tile.Find(".gwios-satellite-pressure-value").GetAttribute("tabindex"));
    }

    [Fact]
    public void DrawsNoPressureHistory_WhenThereIsOnlyOneValue()
    {
        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(_entries.CreateOnline("keller"));

        Assert.Empty(tile.FindAll(".gwios-satellite-pressure-history"));
    }

    [Theory]
    [InlineData(1011.0, "-90", "strong-up", "stark steigend: +3,2 hPa in 3 h")]
    [InlineData(1013.5, "-45", "up", "steigend: +0,7 hPa in 3 h")]
    [InlineData(1014.24, "0", "flat", "gleichbleibend: 0,0 hPa in 3 h")]
    [InlineData(1015.0, "45", "down", "fallend: -0,8 hPa in 3 h")]
    [InlineData(1017.0, "90", "strong-down", "stark fallend: -2,8 hPa in 3 h")]
    public void ShowsTheTendencyOverThreeHoursAsATurnedArrow(
        double pressureThreeHoursAgo,
        string expectedRotation,
        string expectedDirection,
        string expectedTitle)
    {
        SatelliteOverviewEntry entry = _entries.CreateOnline("keller");
        AddPressureHistory(entry, [(-180, pressureThreeHoursAgo)]);

        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(entry);

        AngleSharp.Dom.IElement trend = tile.Find(".gwios-satellite-pressure-trend");
        Assert.Contains($"gwios-satellite-pressure-trend--{expectedDirection}", trend.ClassList);
        Assert.Equal(expectedTitle, trend.GetAttribute("title"));
        Assert.Equal($"rotate({expectedRotation} 12 12)", trend.QuerySelector(".gwios-trend-arrow g")!.GetAttribute("transform"));
        Assert.Equal(expectedTitle, trend.QuerySelector(".gwios-trend-arrow")!.GetAttribute("aria-label"));
    }

    [Fact]
    public void LeavesTheTendencyOpen_WhenTheValuesCoverLessThanTenMinutes()
    {
        SatelliteOverviewEntry entry = _entries.CreateOnline("keller");
        AddPressureHistory(entry, [(-5, 1000.0)]);

        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(entry);

        AngleSharp.Dom.IElement trend = tile.Find(".gwios-satellite-pressure-trend");
        Assert.Equal("·", trend.TextContent.Trim());
        Assert.Empty(trend.QuerySelectorAll(".gwios-trend-arrow"));
        Assert.Contains("gwios-satellite-pressure-trend--none", trend.ClassList);
        Assert.Equal(
            "Tendenz noch unbekannt, dafür fehlen Messwerte aus mindestens 10 Minuten",
            trend.GetAttribute("title"));
    }

    [Fact]
    public void LeavesOutValuesOlderThanThreeHoursFromTheTendency()
    {
        SatelliteOverviewEntry entry = _entries.CreateOnline("keller");
        AddPressureHistory(entry, [(-300, 900.0), (-60, 1013.24)]);

        IRenderedComponent<GwiOSSatelliteTileUnderTest> tile = RenderTile(entry);

        Assert.Equal("stark steigend: +3,0 hPa in 3 h", tile.Find(".gwios-satellite-pressure-trend").GetAttribute("title"));
    }

    // Adds the given values to the long history of the pressure, each measured the given minutes before now.
    private static void AddPressureHistory(SatelliteOverviewEntry entry, (int Minutes, double Pressure)[] values)
    {
        MeasurementHistory pressure = entry.Histories.Single(history => history.Quantity == "pressure");
        pressure.LongTermSamples.AddRange(values.Select(value => new MeasurementSample(
            value.Pressure,
            TestSatelliteEntries.Now.AddMinutes(value.Minutes),
            TestSatelliteEntries.Now.AddMinutes(value.Minutes))));
    }

    private IRenderedComponent<GwiOSSatelliteTileUnderTest> RenderTile(SatelliteOverviewEntry entry)
        => Render<GwiOSSatelliteTileUnderTest>(parameters => parameters
            .Add(component => component.Entry, entry)
            .Add(component => component.Now, TestSatelliteEntries.Now));
}
