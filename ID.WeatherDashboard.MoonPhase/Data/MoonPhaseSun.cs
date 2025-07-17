using System.Text.Json.Serialization;

namespace ID.WeatherDashboard.MoonPhase.Data
{
    public class MoonPhaseSun
    {
        [JsonPropertyName("sunrise")]
        public long? Sunrise { get; set; }

        [JsonPropertyName("sunrise_timestamp")]
        public string? SunriseTimestamp { get; set; }

        [JsonPropertyName("sunset")]
        public long? Sunset { get; set; }

        [JsonPropertyName("sunset_timestamp")]
        public string? SunsetTimestamp { get; set; }

        [JsonPropertyName("solar_noon")]
        public string? SolarNoon { get; set; }

        [JsonPropertyName("day_length")]
        public string? DayLength { get; set; }

        [JsonPropertyName("sun_altitude")]
        public double? SunAltitude { get; set; }

        [JsonPropertyName("sun_distance")]
        public double? SunDistance { get; set; }

        [JsonPropertyName("sun_azimuth")]
        public double? SunAzimuth { get; set; }

        [JsonPropertyName("next_solar_eclipse")]
        public MoonPhaseNextSolarEclipse? NextSolarEclipse { get; set; }
    }
}
