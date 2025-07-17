using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.MoonPhase.Data
{
    public abstract class MoonPhaseStamped
    {
        [JsonPropertyName("timestamp")]
        public long? Timestamp { get; set; }
        [JsonPropertyName("datestamp")]
        public string? Datestamp { get; set; }
    }
}
