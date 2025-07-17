using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Config
{
    public class ServiceElementConfig
    {
        public required string ServiceName { get; set; }
        public required string Action { get; set; }
        public int Weight { get; set; } = 100;
    }
}
