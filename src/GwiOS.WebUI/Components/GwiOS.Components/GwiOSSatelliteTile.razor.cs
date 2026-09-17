using System.Globalization;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts.Models;
using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// Tile of a satellite on the satellite overview: name, online state, the current temperature with its trend over the
/// last hour and its short history, humidity, air pressure, signal strength and the time of the latest ping. An
/// offline satellite shows placeholders instead of values.
/// </summary>
public partial class GwiOSSatelliteTile
{
    private const string TemperatureQuantity = "temperature";
    private const string HumidityQuantity = "humidity";
    private const string PressureQuantity = "pressure";

    // The pressure bar spans 990 to 1040 hPa, the range of air pressure at the height of the house.
    private const double PressureBarMinimum = 990;
    private const double PressureBarRange = 50;

    // The spark bars span at least this many kelvin, so noise of a tenth of a degree shows as a small step and not as a
    // jump from the bottom to the top.
    private const double MinimumSparkSpan = 0.5;

    // Magnus formula for the saturation vapour pressure over water in hPa, and the factor that turns vapour pressure in
    // hPa per kelvin into grams of water per cubic metre (100 / specific gas constant of water vapour, 461.5 J/(kg·K),
    // times 1000 g/kg). Accurate to about 0.1 g/m³ between -30 and 35 °C.
    private const double MagnusFactor = 6.112;
    private const double MagnusExponentFactor = 17.67;
    private const double MagnusTemperatureOffset = 243.5;
    private const double VapourDensityFactor = 216.74;
    private const double KelvinOffset = 273.15;
    private const string CelsiusUnit = "°C";

    private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");
    private static readonly TimeSpan TrendPeriod = TimeSpan.FromHours(1);

    /// <summary>
    /// The satellite the tile shows, with its presence and histories.
    /// </summary>
    [Parameter, EditorRequired]
    public SatelliteOverviewEntry Entry { get; set; } = default!;

    /// <summary>
    /// The current point in time, in UTC, which ages and the trend period are measured against.
    /// </summary>
    [Parameter, EditorRequired]
    public DateTime Now { get; set; }

    [Inject]
    private TimeProvider TimeProvider { get; set; } = default!;

    private string Title
        => Entry.Satellite.DisplayName ?? Entry.Satellite.Name;

    private string Subtitle
        => Entry.Presence is null ? Entry.Satellite.Name : $"{Entry.Satellite.Name} · {Entry.Presence.Board}";

    private string Host
        => Uri.TryCreate(Entry.Presence?.Endpoint, UriKind.Absolute, out Uri? uri) ? uri.Host : "–";

    private double? Temperature
        => GetCurrentValue(TemperatureQuantity);

    private string TemperatureText
        => Temperature is { } temperature ? $"{FormatOneDecimal(temperature)}°" : "––,–°";

    // The same thresholds as the design template: cold below 17 °C, comfortable up to 24 °C, warm up to 28 °C.
    private string HeatLevel
        => Temperature switch
        {
            null => "none",
            >= 28 => "hot",
            >= 24 => "warm",
            >= 17 => "comfortable",
            _ => "cold"
        };

    private double? TemperatureTrend
        => Temperature is { } temperature && GetEarliestSampleOfTrendPeriod() is { } earliestSample
            ? Math.Round(temperature - earliestSample.Value, 1)
            : null;

    private string TrendText
        => TemperatureTrend switch
        {
            null => "keine Daten",
            > 0 => $"▲ +{FormatOneDecimal(TemperatureTrend.Value)} K / 1 h",
            < 0 => $"▼ {FormatOneDecimal(TemperatureTrend.Value)} K / 1 h",
            _ => $"► {FormatOneDecimal(0)} K / 1 h"
        };

    private string TrendDirection
        => TemperatureTrend switch
        {
            null => "none",
            > 0 => "up",
            < 0 => "down",
            _ => "flat"
        };

    private List<SparkBar> SparkBars
        => FindHistory(TemperatureQuantity) is { } history ? CreateSparkBars(history) : [];

    private double? Humidity
        => GetCurrentValue(HumidityQuantity);

    private string HumidityText
        => Humidity is { } humidity ? $"{Math.Round(humidity, MidpointRounding.AwayFromZero)} %" : "–– %";

    // The absolute humidity is derived from temperature and relative humidity, because the sensor measures only the
    // relative humidity. It needs a temperature in degrees Celsius.
    private string? AbsoluteHumidityText
        => (Humidity is { } humidity)
            && (Temperature is { } temperature)
            && (FindHistory(TemperatureQuantity)?.Unit == CelsiusUnit)
                ? $"{FormatOneDecimal(CalculateAbsoluteHumidity(temperature, humidity))} g/m³"
                : null;

    private string HumidityBarStyle
        => CreateWidthStyle(Humidity is { } humidity ? Math.Clamp(humidity, 0, 100) : 0);

    private double? Pressure
        => GetCurrentValue(PressureQuantity);

    private string PressureText
        => Pressure is { } pressure ? $"{FormatOneDecimal(pressure)} hPa" : "–––– hPa";

    private string PressureBarStyle
        => CreateWidthStyle(Pressure is { } pressure
            ? Math.Clamp(Math.Round((pressure - PressureBarMinimum) / PressureBarRange * 100), 4, 100)
            : 0);

    // Values of an offline satellite are outdated, so they are not shown.
    private double? GetCurrentValue(string quantity)
        => Entry.IsOnline && FindHistory(quantity) is { RecentSamples.Count: > 0 } history
            ? history.RecentSamples[^1].Value
            : null;

    private MeasurementHistory? FindHistory(string quantity)
        => Entry.Histories.FirstOrDefault(history => history.Quantity == quantity);

    // The trend compares the current value with the oldest value measured within the last hour, taken from both
    // histories, because the short history only reaches back five minutes.
    private MeasurementSample? GetEarliestSampleOfTrendPeriod()
        => FindHistory(TemperatureQuantity) is { } history
            ? history.LongTermSamples
                .Concat(history.RecentSamples)
                .Where(sample => sample.MeasuredAt >= Now - TrendPeriod)
                .OrderBy(sample => sample.MeasuredAt)
                .FirstOrDefault()
            : null;

    // Bar heights as in the design template: the lowest value gets 22 %, the highest 100 % of 32 px, and older bars
    // fade out. Heights are based on the values as shown, rounded to one decimal, so equal values get equal bars; a
    // range narrower than the minimum span is widened around its middle. The tooltip of a bar names its value and the
    // time it was measured.
    private List<SparkBar> CreateSparkBars(MeasurementHistory history)
    {
        List<MeasurementSample> samples = history.RecentSamples;
        if (samples.Count == 0)
        {
            return [];
        }

        List<double> shownValues = [.. samples.Select(sample => RoundToOneDecimal(sample.Value))];
        (double lowest, double highest) = GetSparkRange(shownValues);
        return
        [
            .. samples.Select((sample, index) => new SparkBar(
                CreateSparkBarStyle(shownValues[index], index, samples.Count, lowest, highest),
                CreateSparkBarTitle(sample, history.Unit)))
        ];
    }

    private static (double Lowest, double Highest) GetSparkRange(List<double> values)
    {
        double lowest = values.Min();
        double highest = values.Max();
        if (highest - lowest >= MinimumSparkSpan)
        {
            return (lowest, highest);
        }

        double middle = (lowest + highest) / 2;
        return (middle - (MinimumSparkSpan / 2), middle + (MinimumSparkSpan / 2));
    }

    private static string CreateSparkBarStyle(double value, int index, int count, double lowest, double highest)
    {
        double percent = 22 + ((value - lowest) / (highest - lowest) * 78);
        double opacity = 0.35 + ((double)index / count * 0.65);
        return string.Create(
            CultureInfo.InvariantCulture,
            $"height:{Math.Round(percent * 0.32, MidpointRounding.AwayFromZero)}px;opacity:{opacity:0.###}");
    }

    private string CreateSparkBarTitle(MeasurementSample sample, string unit)
    {
        DateTimeOffset measuredAt = TimeZoneInfo.ConvertTime(ToUtcOffset(sample.MeasuredAt), TimeProvider.LocalTimeZone);
        return $"{FormatOneDecimal(sample.Value)} {unit} · {measuredAt.ToString("HH:mm:ss", CultureInfo.InvariantCulture)}";
    }

    private static string CreateWidthStyle(double percent)
        => string.Create(CultureInfo.InvariantCulture, $"width:{percent:0.#}%");

    private static double CalculateAbsoluteHumidity(double temperatureCelsius, double relativeHumidityPercent)
    {
        double saturationVapourPressure = MagnusFactor
            * Math.Exp(MagnusExponentFactor * temperatureCelsius / (temperatureCelsius + MagnusTemperatureOffset));
        double vapourPressure = saturationVapourPressure * relativeHumidityPercent / 100;
        return vapourPressure * VapourDensityFactor / (temperatureCelsius + KelvinOffset);
    }

    private static double RoundToOneDecimal(double value)
        => Math.Round(value, 1, MidpointRounding.AwayFromZero);

    private static string FormatOneDecimal(double value)
        => value.ToString("0.0", GermanCulture);

    private static DateTimeOffset ToUtcOffset(DateTime utcDateTime)
        => new(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc));

    private sealed record SparkBar(string Style, string Title);
}
