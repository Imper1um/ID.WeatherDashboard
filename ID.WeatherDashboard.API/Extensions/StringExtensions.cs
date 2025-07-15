
using ID.WeatherDashboard.API.Data;

public static class StringExtensions
{
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

