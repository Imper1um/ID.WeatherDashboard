using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.SunriseSunset.Data
{
    public class SunriseSunsetResult
    {
        public DateTimeOffset Pulled { get; set; } = DateTimeOffset.Now;

        [JsonPropertyName("sunrise")]
        public string? Sunrise { get; set; }
        [JsonPropertyName("sunset")]
        public string? Sunset { get; set; }
        [JsonPropertyName("solar_noon")]
        public string? SolarNoon { get; set; }
        [JsonPropertyName("day_length")]
        public int? DayLength { get; set; }
        [JsonPropertyName("civil_twilight_begin")]
        public string? CivilTwilightBegin { get; set; }
        [JsonPropertyName("civil_twilight_end")]
        public string? CivilTwilightEnd { get; set; }
        [JsonPropertyName("nautical_twilight_begin")]   
        public string? NauticalTwilightBegin { get; set; }
        [JsonPropertyName("nautical_twilight_end")]
        public string? NauticalTwilightEnd { get; set; }    
        [JsonPropertyName("astronomical_twilight_begin")]
        public string? AstronomicalTwilightBegin { get; set; }
        [JsonPropertyName("astronomical_twilight_end")]
        public string? AstronomicalTwilightEnd { get; set; }
    }
}
