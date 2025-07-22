/// <summary>
///     Provides extension methods for working with <see cref="DateTimeOffset"/> values.
/// </summary>
public static class DateTimeOffsetExtensions
{
    /// <summary>
    ///     Truncates the <see cref="DateTimeOffset"/> to the hour component.
    /// </summary>
    /// <param name="value">The value to truncate.</param>
    /// <returns>The truncated <see cref="DateTimeOffset"/>.</returns>
    public static DateTimeOffset HourOf(this DateTimeOffset value)
    {
        return new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, 0, 0, value.Offset);
    }

    /// <summary>
    ///     Truncates the <see cref="DateTimeOffset"/> to the day component.
    /// </summary>
    /// <param name="value">The value to truncate.</param>
    /// <returns>The truncated <see cref="DateTimeOffset"/>.</returns>
    public static DateTimeOffset DayOf(this DateTimeOffset value)
    {
        return new DateTimeOffset(value.Year, value.Month, value.Day, 0, 0, 0, value.Offset);
    }
}
