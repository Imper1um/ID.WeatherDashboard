using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    public abstract class DataLine
    {
        protected DataLine(DateTimeOffset pulled, params string[] sources)
        {
            Pulled = pulled;
            Sources = sources ?? Array.Empty<string>();
        }

        public DateTimeOffset Pulled { get; set; }
        public DateTimeOffset? Observed { get; set; }
        public string[] Sources { get; set; }
        public string? StationId { get; set; }
        public WindDirection? WindDirection { get; set; }
        public float? Humidity { get; set; }
        public Temperature? CurrentTemperature { get; set; }
        public Temperature? FeelsLike { get; set; }
        public Temperature? HeatIndex { get; set; }
        public Temperature? DewPoint { get; set; }
        public float? UVIndex { get; set; }
        public Pressure? Pressure { get; set; }
        public Coordinates? Coordinates { get; set; }
        public WeatherConditions? WeatherConditions { get; set; }
    }
}
