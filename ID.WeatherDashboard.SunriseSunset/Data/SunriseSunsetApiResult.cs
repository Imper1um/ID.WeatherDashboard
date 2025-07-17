using ID.WeatherDashboard.API.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.SunriseSunset.Data
{
    public class SunriseSunsetApiResult
    {
        public DateTimeOffset Pulled { get; set; } = DateTimeOffset.Now;

        [JsonPropertyName("results")]
        public SunriseSunsetResult? Results { get; set; }
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        [JsonPropertyName("tzid")]
        public string? TimezoneId { get; set; }

        public DateTimeOffset? DateToDateTimeOffset(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;
            try
            {
                return DateTimeOffset.Parse(dateString, null, DateTimeStyles.RoundtripKind);
            } 
            catch (FormatException)
            {
            }
            return null;
        }

        public SunLine ToSunLine(double? latitude)
        {
            return new SunLine(Pulled, "SunriseSunset")
            {
                Sunrise = DateToDateTimeOffset(Results?.Sunrise),
                Sunset = DateToDateTimeOffset(Results?.Sunset),
                SolarNoon = DateToDateTimeOffset(Results?.SolarNoon),
                CivilTwilightBegin = DateToDateTimeOffset(Results?.CivilTwilightBegin),
                CivilTwilightEnd = DateToDateTimeOffset(Results?.CivilTwilightEnd),
                NauticalTwilightBegin = DateToDateTimeOffset(Results?.NauticalTwilightBegin),
                NauticalTwilightEnd = DateToDateTimeOffset(Results?.NauticalTwilightEnd),
                AstronomicalTwilightBegin = DateToDateTimeOffset(Results?.AstronomicalTwilightBegin),
                AstronomicalTwilightEnd = DateToDateTimeOffset(Results?.AstronomicalTwilightEnd),
                Latitude = latitude
            };
        }
    }
}
