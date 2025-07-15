using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    public class SunLine
    {
        public SunLine(DateTimeOffset pulled, params string[] sources)
        {
            Pulled = pulled;
            Sources = sources;
        }

        public DateTimeOffset Pulled { get; set; }
        public string[] Sources { get; set; }

        public DateTimeOffset? Sunrise { get; set; }
        public DateTimeOffset? Sunset { get; set; }
        public DateTimeOffset? SolarNoon { get; set; }
        public DateTimeOffset? CivilTwilightBegin { get; set; }
        public DateTimeOffset? CivilTwilightEnd { get; set; }
        public DateTimeOffset? NauticalTwilightBegin { get; set; }
        public DateTimeOffset? NauticalTwilightEnd { get; set; }
        public DateTimeOffset? AstronomicalTwilightBegin { get; set; }
        public DateTimeOffset? AstronomicalTwilightEnd { get; set; }
        public DateTimeOffset? Moonrise { get; set; }
        public DateTimeOffset? Moonset { get; set; }
        public MoonPhase? MoonPhase { get; set; }
        public double? Latitude { get; set; }

        public DateTime For { get; set; }
    }
}
