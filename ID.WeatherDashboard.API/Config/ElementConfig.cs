using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Config
{
    public class ElementConfig
    {
        public required string Name { get; set; }
        public List<ServiceElementConfig> ServiceElements { get; set; } = new List<ServiceElementConfig>();
    }
}
