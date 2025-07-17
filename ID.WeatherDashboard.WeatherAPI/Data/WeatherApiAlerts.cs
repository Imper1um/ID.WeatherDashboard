using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.WeatherAPI.Data
{
    public class WeatherApiAlerts
    {
        [JsonPropertyName("alert")]
        public List<WeatherApiAlert> Alerts { get; set; } = new List<WeatherApiAlert>();
    }
}
