using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.MoonPhase.Data
{
    public class MoonPhaseNextLunarEclipse : MoonPhaseStamped
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }
        [JsonPropertyName("visibility_regions")]
        public string? VisibilityRegions { get; set; }
    }
}
