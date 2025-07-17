using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.WUnderground.Data
{
    public class Imperial
    {
        public float? Temperature { get; set; } // Fahrenheit
        public float? HeatIndex { get; set; } // Fahrenheit  
        public float? DewPoint { get; set; } // Fahrenheit
        public float? WindChill { get; set; } // Fahrenheit
        public float? WindSpeed { get; set; } // Miles per hour
        public float? WindGust { get; set; } // Miles per hour
        public float? Pressure { get; set; } // Inches of mercury
        public float? PrecipitationRate { get; set; } // Inches
        public float? PrecipitationTotal { get; set; } // Inches
        public float? Elevation { get; set; }
    }
}
