using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.WeatherAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.WeatherAPI.Data
{
    public class WeatherApiForecastAPI
    {
        [JsonPropertyName("location")]
        public WeatherApiLocation? Location { get; set; }
        [JsonPropertyName("current")]
        public WeatherApiCurrent? Current { get; set; }
        [JsonPropertyName("forecast")]
        public WeatherApiForecast? Forecast { get; set; }

        public DateTimeOffset Pulled { get; set; } = DateTimeOffset.Now;

        public ForecastData? ToForecastData()
        {
            if (Forecast == null)
                return null;
            var lines = from d in Forecast?.ForecastDays
                        select d.ToForecastDay();
            var fd = new ForecastData(Pulled, lines);
            return fd;
        }
    }
}
