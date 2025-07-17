using ID.WeatherDashboard.API.Data;
using System.Text.Json.Serialization;

namespace ID.WeatherDashboard.MoonPhase.Data
{
    public class MoonPhaseAdvancedAPI : MoonPhaseStamped
    {
        public MoonPhaseAdvancedAPI(DateTime forDateTime, DateTimeOffset? pulled = null)
        {
            For = forDateTime;
            if (pulled != null)
                Pulled = pulled.Value;
        }

        public DateTime For { get; set; }

        public DateTimeOffset Pulled { get; set; } = DateTimeOffset.Now;
        [JsonPropertyName("sun")]
        public MoonPhaseSun? Sun { get; set; }
        [JsonPropertyName("moon")]
        public MoonPhaseMoon? Moon { get; set; }
        [JsonPropertyName("moon_phases")]
        public MoonPhaseMoonPhases? MoonPhases { get; set; }
        [JsonPropertyName("location")]
        public MoonPhaseLocation? Location { get; set; }

        private DateTimeOffset? ParseTime(string? time, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(time))
                return null;
            if (TimeSpan.TryParse(time, out var ts))
            {
                return new DateTimeOffset(date.Year, date.Month, date.Day, ts.Hours, ts.Minutes, 0, TimeSpan.Zero);
            }
            return null;
        }

        public MoonData? ToMoonData()
        {
            if (Moon == null || Location == null)
                return null;
            var moonData = new MoonData(Pulled, "MoonPhase")
            {
                For = For,
                Latitude = Location.Latitude,
                Longitude = Location.Longitude,
                Moonrise = Moon.MoonriseTimestamp != null ? DateTimeOffset.FromUnixTimeSeconds(Moon.MoonriseTimestamp.Value) : null,
                Moonset = Moon.MoonsetTimestamp != null ? DateTimeOffset.FromUnixTimeSeconds(Moon.MoonsetTimestamp.Value) : null,
                MoonPhase = Moon.PhaseName?.ToMoonPhase(),
                MoonAzimuth = Moon.MoonAzimuth != null ? new MoonProperty(Moon.MoonAzimuth.Value, DateTimeOffset.FromUnixTimeSeconds(Timestamp ?? 0)) : null,
                MoonParallacticAngle = Moon.MoonParallacticAngle != null ? new MoonProperty(Moon.MoonParallacticAngle.Value, DateTimeOffset.FromUnixTimeSeconds(Timestamp ?? 0)) : null,
                MoonDistance = Moon.MoonDistance != null ? new MoonProperty(Moon.MoonDistance.Value, DateTimeOffset.FromUnixTimeSeconds(Timestamp ?? 0)) : null,
                MoonAltitude = Moon.MoonAltitude != null ? new MoonProperty(Moon.MoonAltitude.Value, DateTimeOffset.FromUnixTimeSeconds(Timestamp ?? 0)) : null,

            };
            moonData.MoonDeclination = moonData.CalculateDeclination();
            return moonData;
        }

        public SunData? ToSunData(DateTime forDateTime)
        {
            if (Location == null || (Sun == null && Moon == null))
                return null;

            return new SunData(Pulled,
                new SunLine(Pulled, "MoonPhase")
                {
                    For = forDateTime.Date,
                    Sunrise = Sun?.Sunrise != null ? DateTimeOffset.FromUnixTimeSeconds(Sun.Sunrise.Value) : null,
                    Sunset = Sun?.Sunset != null ? DateTimeOffset.FromUnixTimeSeconds(Sun.Sunset.Value) : null,
                    SolarNoon = Sun?.SolarNoon != null ? ParseTime(Sun.SolarNoon, forDateTime.Date) : null,
                    Latitude = Location.Latitude,
                    MoonData = ToMoonData()

                })
            {
                Sources = new[] { "MoonPhase" }
            };
        }
    }
}
