using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Config
{
    public class DataConfig
    {
        public List<ElementConfig> Elements { get; set; } = [];
        public TimeSpan? MaxDataAge { get; set; }
        public string? RefreshEvent { get; set; }
        public bool OverlayExistingData { get; set; } = true;
    }
}
