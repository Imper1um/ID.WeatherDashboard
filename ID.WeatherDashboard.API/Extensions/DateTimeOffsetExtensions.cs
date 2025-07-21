public static class DateTimeOffsetExtensions
{
    public static DateTimeOffset MinuteOf(this DateTimeOffset value)
    {
        return new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0, value.Offset);
    }

    public static DateTimeOffset HourOf(this DateTimeOffset value)
    {
        return new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, 0, 0, value.Offset);
    }

    public static DateTimeOffset DayOf(this DateTimeOffset value)
    {
        return new DateTimeOffset(value.Year, value.Month, value.Day, 0, 0, 0, value.Offset);
    }
} 