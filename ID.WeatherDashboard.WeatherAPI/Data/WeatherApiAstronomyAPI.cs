using ID.WeatherDashboard.API.Data;
using System.Text.Json.Serialization;

namespace ID.WeatherDashboard.WeatherAPI.Data
{
    public class WeatherApiAstronomyAPI
    {
        public WeatherApiAstronomyAPI(DateTime forDt)
        {
            For = forDt;
        }

        public DateTimeOffset Pulled { get; set; } = DateTimeOffset.Now;
        public DateTime For { get; set; }

        [JsonPropertyName("location")]
        public WeatherApiLocation? Location { get; set; }
        [JsonPropertyName("astronomy")]
        public WeatherApiAstronomy? Astronomy { get; set; }

        private DateTimeOffset? TimeFromAstronomy(string timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
                return null;

            var timezoneId = Location?.TimezoneId ?? "UTC";
            var forDate = For;

            if (!DateTime.TryParseExact(timeString, "hh:mm tt", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedTime))
                return null;

            var localDateTime = new DateTime(forDate.Year, forDate.Month, forDate.Day, parsedTime.Hour, parsedTime.Minute, 0, DateTimeKind.Unspecified);

            TimeZoneInfo tz;
            try
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                tz = TimeZoneInfo.Utc;
            }
            catch (InvalidTimeZoneException)
            {
                tz = TimeZoneInfo.Utc;
            }
            var offset = tz.GetUtcOffset(localDateTime);

            return new DateTimeOffset(localDateTime, offset);
        }

        public MoonData? ToMoonData()
        {
            if (Location == null || Astronomy?.Astro == null)
                return null;
            return new MoonData(Pulled, "WeatherAPI")
            {
                Moonrise = string.IsNullOrWhiteSpace(Astronomy.Astro.Moonrise) ? null : TimeFromAstronomy(Astronomy.Astro.Moonrise!),
                Moonset = string.IsNullOrWhiteSpace(Astronomy.Astro.Moonset) ? null : TimeFromAstronomy(Astronomy.Astro.Moonset!),
                MoonPhase = Astronomy.Astro.MoonPhase?.ToMoonPhase(),
                For = For,
                Latitude = Location.Latitude,
            };
        }

        public SunData? ToSunData()
        {
            if (Location == null || Astronomy?.Astro == null)
                return null;

            return new SunData(Pulled,
                new SunLine(Pulled, "WeatherAPI")
                {
                    Sunrise = string.IsNullOrWhiteSpace(Astronomy.Astro.Sunrise) ? null : TimeFromAstronomy(Astronomy.Astro.Sunrise!),
                    Sunset = string.IsNullOrWhiteSpace(Astronomy.Astro.Sunset) ? null : TimeFromAstronomy(Astronomy.Astro.Sunset!),
                    MoonData = ToMoonData(),
                    For = For,
                    Latitude = Location.Latitude,
                })
            {
                Sources = new[] { "WeatherAPI" }
            };
        }
    }
}
