namespace GwiOS.WebUI.Tests.TestInfrastructure;

/// <summary>
/// Stands in for <see cref="TimeProvider"/> with a clock that stands still: it always reports
/// <see cref="UtcNow"/>, which tests may set, and uses <see cref="LocalTimeZone"/> as local time zone.
/// </summary>
public sealed class TimeProviderFake : TimeProvider
{
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.Utc;

    /// <summary>
    /// The point in time the clock reports. Defaults to 2026-07-14 10:30 UTC.
    /// </summary>
    public DateTimeOffset UtcNow { get; set; } = new(2026, 7, 14, 10, 30, 0, TimeSpan.Zero);

    /// <summary>
    /// The local time zone of the clock. Defaults to UTC; tests may set any other zone.
    /// </summary>
    public override TimeZoneInfo LocalTimeZone
        => _localTimeZone;

    /// <summary>
    /// Makes the clock use a local time zone that is the given offset ahead of UTC.
    /// </summary>
    public void UseLocalOffset(TimeSpan offset)
        => _localTimeZone = TimeZoneInfo.CreateCustomTimeZone("Test", offset, "Test", "Test");

    public override DateTimeOffset GetUtcNow()
        => UtcNow;
}
