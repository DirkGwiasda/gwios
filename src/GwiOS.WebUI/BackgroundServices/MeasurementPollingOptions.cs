namespace GwiOS.WebUI.BackgroundServices;

/// <summary>
/// Settings of <see cref="MeasurementPollingService"/>, read from the configuration section
/// <see cref="SectionName"/>.
/// </summary>
public class MeasurementPollingOptions
{
    /// <summary>
    /// The configuration section the settings are read from.
    /// </summary>
    public const string SectionName = "Satellites:MeasurementPolling";

    /// <summary>
    /// Seconds between two fetches of the measurements of all satellites. Defaults to 30.
    /// </summary>
    public int IntervalSeconds { get; set; } = 30;
}
