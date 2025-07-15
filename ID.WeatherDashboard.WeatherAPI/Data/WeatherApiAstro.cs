using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.WeatherAPI.Data
{
    public class WeatherApiAstro
    {
        public string? Sunrise { get; set; }
        public string? Sunset { get; set; }
        public string? Moonrise { get; set; }
        public string? Moonset { get; set; }
        public string? MoonPhase { get; set; }
        public float? MoonIllumination { get; set; }
        public int? IsMoonUp { get; set; }
        public int? IsSunUp { get; set; }
    }
}
