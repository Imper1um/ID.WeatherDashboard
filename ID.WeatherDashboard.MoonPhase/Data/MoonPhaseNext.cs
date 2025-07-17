using System.Text.Json.Serialization;

namespace ID.WeatherDashboard.MoonPhase.Data
{
    public class MoonPhaseNext : MoonPhaseStamped
    {
        [JsonPropertyName("days_ahead")]
        public int? DaysAhead { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
