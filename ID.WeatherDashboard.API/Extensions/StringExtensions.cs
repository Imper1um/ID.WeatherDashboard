using ID.WeatherDashboard.API.Data;

/// <summary>
///     Provides extension methods for <see cref="string"/> values.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    ///     Converts a moon phase string into a <see cref="MoonPhase"/> value.
    /// </summary>
    /// <param name="self">The string to parse.</param>
    /// <returns>The parsed <see cref="MoonPhase"/>, or <see langword="null"/> if the value could not be parsed.</returns>
    public static MoonPhase? ToMoonPhase(this string? self)
    {
        if (self == null) return null;
        return self.ToLowerInvariant() switch
        {
            "new moon" => MoonPhase.NewMoon,
            "waxing crescent" => MoonPhase.WaxingCrescent,
            "first quarter" => MoonPhase.FirstQuarter,
            "waxing gibbous" => MoonPhase.WaxingGibbous,
            "full moon" => MoonPhase.FullMoon,
            "waning gibbous" => MoonPhase.WaningGibbous,
            "last quarter" => MoonPhase.LastQuarter,
            "waning crescent" => MoonPhase.WaningCrescent,
            _ => null
        };
    }
}
