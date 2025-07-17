using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Config
{
    public class DashboardConfig
    {
        public List<ServiceConfig> Services { get; set; } = new List<ServiceConfig>();
        public DataConfig CurrentData { get; set; } = new DataConfig();
        public DataConfig HistoryData { get; set; } = new DataConfig();
        public DataConfig ForecastData { get; set; } = new DataConfig();
        public DataConfig SunData { get; set; } = new DataConfig();
        public DataConfig AlertData { get; set; } = new DataConfig();
    }
}
