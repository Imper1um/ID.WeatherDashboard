using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Config
{
    public class BackgroundConfig
    {
        public string BackgroundFolder { get; set; } = string.Empty;
        public TimeSpan? ScanTime { get; set; } = TimeSpan.FromMinutes(5);
    }
}
