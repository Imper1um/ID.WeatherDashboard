using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Config
{
    public class DashboardConfig
    {
        public List<ServiceConfig> Services { get; set; } = [];
        public DataConfig CurrentData { get; set; } = new DataConfig();
        public DataConfig HistoryData { get; set; } = new DataConfig();
        public DataConfig ForecastData { get; set; } = new DataConfig();
        public DataConfig SunData { get; set; } = new DataConfig();
        public DataConfig AlertData { get; set; } = new DataConfig();
        public ChatGptConfig ChatGpt { get; set; } = new() { ApiKey = string.Empty, Model = "gpt-4o" };
        public BackgroundConfig Background { get; set; } = new BackgroundConfig();
        public string Location { get; set; } = string.Empty;
    }
}
