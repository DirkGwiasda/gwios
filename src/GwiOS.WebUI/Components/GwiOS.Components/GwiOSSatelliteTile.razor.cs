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

    // The pressure bar spans the range a barometer covers; satellites report the pressure at their own height, so the
    // mark sits at the local average instead of the 1013 hPa of a barometer reduced to sea level.
    private const double PressureScaleMinimum = 954;
    private const double PressureScaleMaximum = 1060;
    private const double LocalAveragePressure = 996;

    // The line chart spans at least this many hPa, so a quiet day looks flat.
    private const double MinimumPressureSpan = 2;

    // The tendency is the change over three hours, as in meteorology, derived from the values of the last three hours.
    // Below half a hPa per three hours the pressure counts as steady, from two hPa as a strong change.
    private static readonly TimeSpan PressureTendencyPeriod = TimeSpan.FromHours(3);
    private static readonly TimeSpan MinimumPressureTendencySpan = TimeSpan.FromMinutes(10);
    private const double SteadyPressureChange = 0.5;
    private const double StrongPressureChange = 2;

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
        => Temperature is { } temperature ? $"{FormatTwoDecimals(temperature)}°" : "––,––°";

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
        => CreateWidthStyle(Pressure is { } pressure ? Math.Clamp(GetPressurePercent(pressure), 4, 100) : 0);

    private string PressureMarkStyle
        => string.Create(CultureInfo.InvariantCulture, $"left:{GetPressurePercent(LocalAveragePressure):0.#}%");

    private string PressureScaleTitle
        => $"Skala {FormatWholeNumber(PressureScaleMinimum)} bis {FormatWholeNumber(PressureScaleMaximum)} hPa, "
            + $"Strich beim örtlichen Mittelwert {FormatWholeNumber(LocalAveragePressure)} hPa";

    // The chart shows everything kept in memory, the long history plus the newer values of the short one. The long
    // history is thinned out, so the points are placed by their time and not evenly.
    private List<GwiOSSparkLinePoint> PressurePoints
    {
        get
        {
            List<MeasurementSample> samples = GetPressureSamples();
            return samples.Count == 0
                ? []
                :
                [
                    .. samples.Select(sample => new GwiOSSparkLinePoint(
                        (sample.MeasuredAt - samples[0].MeasuredAt).TotalMinutes,
                        sample.Value))
                ];
        }
    }

    private string PressureHistoryTitle
        => GetPressureSamples() is [{ } oldest, .., { } latest]
            ? $"Verlauf von {FormatOneDecimal(oldest.Value)} bis {FormatOneDecimal(latest.Value)} hPa "
                + $"über {DescribeSpan(latest.MeasuredAt - oldest.MeasuredAt)}"
            : "Verlauf des Luftdrucks";

    // The change over three hours, extrapolated from the values of the last three hours; null until the values cover
    // at least ten minutes, because the sensor alone varies by about a tenth of a hPa.
    private double? PressureTendency
    {
        get
        {
            List<MeasurementSample> samples =
                [.. GetPressureSamples().Where(sample => sample.MeasuredAt >= Now - PressureTendencyPeriod)];
            if (samples is not [{ } oldest, .., { } latest])
            {
                return null;
            }

            TimeSpan span = latest.MeasuredAt - oldest.MeasuredAt;
            return span < MinimumPressureTendencySpan
                ? null
                : (latest.Value - oldest.Value) / span.TotalHours * PressureTendencyPeriod.TotalHours;
        }
    }

    // The arrow points right when the pressure is steady, and turns up or down with the tendency.
    private double? PressureTrendRotation
        => PressureTrendDirection switch
        {
            "strong-up" => -90,
            "up" => -45,
            "down" => 45,
            "strong-down" => 90,
            "flat" => 0,
            _ => null
        };

    private string PressureTrendDirection
        => PressureTendency switch
        {
            null => "none",
            >= StrongPressureChange => "strong-up",
            >= SteadyPressureChange => "up",
            <= -StrongPressureChange => "strong-down",
            <= -SteadyPressureChange => "down",
            _ => "flat"
        };

    private string PressureTrendTitle
        => PressureTendency is { } tendency
            ? $"{DescribeTendency()}: {FormatSignedOneDecimal(tendency)} hPa in 3 h"
            : "Tendenz noch unbekannt, dafür fehlen Messwerte aus mindestens 10 Minuten";

    private string DescribeTendency()
        => PressureTrendDirection switch
        {
            "strong-up" => "stark steigend",
            "up" => "steigend",
            "down" => "fallend",
            "strong-down" => "stark fallend",
            _ => "gleichbleibend"
        };

    // Both histories are merged, because the long one thins out the older values and the short one holds the newest.
    private List<MeasurementSample> GetPressureSamples()
        => FindHistory(PressureQuantity) is { } history
            ?
            [
                .. history.LongTermSamples
                    .Concat(history.RecentSamples)
                    .GroupBy(sample => sample.MeasuredAt)
                    .Select(group => group.First())
                    .OrderBy(sample => sample.MeasuredAt)
            ]
            : [];

    private static double GetPressurePercent(double pressure)
        => Math.Clamp(
            (pressure - PressureScaleMinimum) / (PressureScaleMaximum - PressureScaleMinimum) * 100,
            0,
            100);

    private static string DescribeSpan(TimeSpan span)
        => span.TotalHours >= 1
            ? $"{FormatOneDecimal(span.TotalHours)} h"
            : $"{Math.Round(span.TotalMinutes, MidpointRounding.AwayFromZero)} min";

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
    // fade out. Heights are based on the values as shown, rounded to two decimals, so equal values get equal bars; a
    // range narrower than the minimum span is widened around its middle. The tooltip of a bar names its value and the
    // time it was measured.
    private List<SparkBar> CreateSparkBars(MeasurementHistory history)
    {
        List<MeasurementSample> samples = history.RecentSamples;
        if (samples.Count == 0)
        {
            return [];
        }

        List<double> shownValues = [.. samples.Select(sample => RoundToTwoDecimals(sample.Value))];
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
        return $"{FormatTwoDecimals(sample.Value)} {unit} · {measuredAt.ToString("HH:mm:ss", CultureInfo.InvariantCulture)}";
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

    private static double RoundToTwoDecimals(double value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    // Temperatures are shown with the two decimals the sensor delivers, so the different heights of the spark bars
    // can be read off their values.
    private static string FormatTwoDecimals(double value)
        => value.ToString("0.00", GermanCulture);

    private static string FormatSignedOneDecimal(double value)
        => value.ToString("+0.0;-0.0;0.0", GermanCulture);

    private static string FormatWholeNumber(double value)
        => value.ToString("0", GermanCulture);

    private static string FormatOneDecimal(double value)
        => value.ToString("0.0", GermanCulture);

    private static DateTimeOffset ToUtcOffset(DateTime utcDateTime)
        => new(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc));

    private sealed record SparkBar(string Style, string Title);
}
