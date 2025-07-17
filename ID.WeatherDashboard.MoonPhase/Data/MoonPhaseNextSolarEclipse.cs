using System.Text.Json.Serialization;

namespace ID.WeatherDashboard.MoonPhase.Data
{
    public class MoonPhaseNextSolarEclipse : MoonPhaseStamped
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }
        [JsonPropertyName("visibility_regions")]
        public string? VisibilityRegions { get; set; }
    }
}
