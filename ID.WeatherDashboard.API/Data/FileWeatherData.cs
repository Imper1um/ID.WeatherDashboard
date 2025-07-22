using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    public class FileWeatherData
    {
        public required string Path { get; set; }
        public DateTime Modified { get; set; }
        public required long Size { get; set; }
        public ImageWeatherData? ImageWeatherData { get; set; }
    }
}
