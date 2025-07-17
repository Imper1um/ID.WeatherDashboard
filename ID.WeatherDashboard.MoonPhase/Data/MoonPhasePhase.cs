using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.MoonPhase.Data
{
    public class MoonPhasePhase
    {
        [JsonPropertyName("last")]
        public MoonPhaseLast? Last { get; set; }

        [JsonPropertyName("next")]
        public MoonPhaseNext? Next { get; set; }
    }
}
