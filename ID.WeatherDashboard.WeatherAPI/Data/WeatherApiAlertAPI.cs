using ID.WeatherDashboard.API.Data;
using System.Text.Json.Serialization;

namespace ID.WeatherDashboard.WeatherAPI.Data
{
    public class WeatherApiAlertAPI
    {
        public DateTimeOffset Pulled { get; set; } = DateTimeOffset.Now;

        [JsonPropertyName("location")]
        public WeatherApiLocation? Location { get; set; }
        [JsonPropertyName("alerts")]
        public WeatherApiAlerts? Alerts { get; set; }

        public AlertData? ToAlertData()
        {
            if (Alerts == null) return null;
            return new AlertData(Pulled, Alerts.Alerts.Select(a => a.ToAlert()).ToArray());
        }
    }
}
