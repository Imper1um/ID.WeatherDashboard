using System.Text.Json.Serialization;

namespace ID.WeatherDashboard.MoonPhase.Data
{
    public class MoonPhaseMoon
    {
        [JsonPropertyName("phase")]
        public double? Phase { get; set; }

        [JsonPropertyName("phase_name")]
        public string? PhaseName { get; set; }

        [JsonPropertyName("stage")]
        public string? Stage { get; set; }

        [JsonPropertyName("illumination")]
        public string? Illumination { get; set; }

        [JsonPropertyName("age_days")]
        public int? AgeDays { get; set; }

        [JsonPropertyName("lunar_cycle")]
        public string? LunarCycle { get; set; }

        [JsonPropertyName("emoji")]
        public string? Emoji { get; set; }

        [JsonPropertyName("zodiac")]
        public MoonPhaseZodiac? Zodiac { get; set; }

        [JsonPropertyName("moonrise")]
        public string? Moonrise { get; set; }

        [JsonPropertyName("moonrise_timestamp")]
        public long? MoonriseTimestamp { get; set; }

        [JsonPropertyName("moonset")]
        public string? Moonset { get; set; }

        [JsonPropertyName("moonset_timestamp")]
        public long? MoonsetTimestamp { get; set; }

        [JsonPropertyName("moon_altitude")]
        public double? MoonAltitude { get; set; }

        [JsonPropertyName("moon_distance")]
        public double? MoonDistance { get; set; }

        [JsonPropertyName("moon_azimuth")]
        public double? MoonAzimuth { get; set; }

        [JsonPropertyName("moon_parallactic_angle")]
        public double? MoonParallacticAngle { get; set; }

        [JsonPropertyName("next_lunar_eclipse")]
        public MoonPhaseNextLunarEclipse? NextLunarEclipse { get; set; }
    }
}
