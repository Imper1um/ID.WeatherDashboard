using ID.WeatherDashboard.API.Data;
using System.Text.Json.Serialization;

namespace ID.WeatherDashboard.WeatherAPI.Data
{
    public class WeatherApiHistoryAPI
    {
        public DateTimeOffset Pulled { get; set; } = DateTimeOffset.Now;

        [JsonPropertyName("location")]
        public WeatherApiLocation? Location { get; set; }

        [JsonPropertyName("forecast")]
        public WeatherApiForecast? Forecast { get; set; }

        public HistoryData? ToHistoryData()
        {
            if (Forecast == null || Forecast.ForecastDays == null)
            {
                return null;
            }
            return new HistoryData(Pulled, Forecast?.ForecastDays?.SelectMany(fd => fd.Hours?.Select(h => h.ToHistoryLine()) ?? new HistoryLine[0]).ToArray() ?? new HistoryLine[0])
            {
                Sources = new[] {"WeatherApi"}
            };
        }
    }
}
