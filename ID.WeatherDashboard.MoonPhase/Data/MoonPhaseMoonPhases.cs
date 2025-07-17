using System.Text.Json.Serialization;

namespace ID.WeatherDashboard.MoonPhase.Data
{
    public class MoonPhaseMoonPhases
    {
        [JsonPropertyName("new_moon")]
        public MoonPhasePhase? NewMoon { get; set; }

        [JsonPropertyName("first_quarter")]
        public MoonPhasePhase? FirstQuarter { get; set; }

        [JsonPropertyName("full_moon")]
        public MoonPhasePhase? FullMoon { get; set; }

        [JsonPropertyName("last_quarter")]
        public MoonPhasePhase? LastQuarter { get; set; }
    }
    
    
    
}
