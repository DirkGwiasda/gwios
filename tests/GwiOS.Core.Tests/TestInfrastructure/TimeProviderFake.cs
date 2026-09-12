namespace GwiOS.Core.Tests.TestInfrastructure;

/// <summary>
/// Stands in for <see cref="TimeProvider"/> with a clock that stands still: it always reports
/// <see cref="UtcNow"/>, which tests may set, and uses UTC as the local time zone.
/// </summary>
public sealed class TimeProviderFake : TimeProvider
{
    /// <summary>
    /// The point in time the clock reports. Defaults to 2026-07-14 10:30 UTC.
    /// </summary>
    public DateTimeOffset UtcNow { get; set; } = new(2026, 7, 14, 10, 30, 0, TimeSpan.Zero);

    public override TimeZoneInfo LocalTimeZone
        => TimeZoneInfo.Utc;

    public override DateTimeOffset GetUtcNow()
        => UtcNow;
}
