namespace ItPlanetAPI.Extensions;

public static class DateTimeExtensions
{
    public static DateTimeOffset Truncate(this DateTimeOffset dateTime, TimeSpan timeSpan)
    {
        if (timeSpan == TimeSpan.Zero) return dateTime;
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue) return dateTime;
        return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
    }

    public static DateTimeOffset AsWholeSeconds(this DateTimeOffset dateTime)
    {
        return dateTime.Truncate(TimeSpan.FromSeconds(1));
    }
}