using ID.WeatherDashboard.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Elements.Background
{
    public class BackgroundSelection
    {
        public required string Path { get; set; }
        public ImageWeatherData? WeatherData { get; set; }
    }
}
