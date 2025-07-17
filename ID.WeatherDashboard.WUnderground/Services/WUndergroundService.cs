using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.WUnderground.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.WUnderground.Services
{
    public class WUndergroundService : ICurrentQueryService, IHistoryQueryService
    {
        public const string DefaultServiceName = "WUnderground";
        public const string _historyUrl = "https://api.weather.com/v2/pws/history/all";
        public const string _currentUrl = "https://api.weather.com/v2/pws/observations/current";

        public string ApiKey => Config?.ApiKey ?? string.Empty;
        public string StationId => Config?.StationId ?? string.Empty;
        public WUndergroundApiConfig? Config { get; set; }
        public string ServiceName => Config?.Name ?? DefaultServiceName;

        public async Task<CurrentData?> GetCurrentDataAsync(Location location)
        {
            using var httpClient = new HttpClient();
            var url = $"{_currentUrl}?stationId={StationId}&format=json&units=e&apiKey={ApiKey}&numericPrecision=decimal";
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var currentData = JsonSerializer.Deserialize<Observations>(content);
                return currentData?.ToCurrentData();
            }
            return null;
        }

        public async Task<HistoryData?> GetHistoryDataAsync(Location location, DateTimeOffset from, DateTimeOffset to)
        {
            using var httpClient = new HttpClient();
            var url = $"{_historyUrl}?stationId={StationId}&format=json&units=e&apiKey={ApiKey}&numericPrecision=decimal&startDate={from:yyyyMMdd}&endDate={to:yyyyMMdd}";
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var historyData = JsonSerializer.Deserialize<Observations>(content);
                return historyData?.ToHistoryData();
            }
            return null;
        }

        public void SetServiceConfig(ServiceConfig config)
        {
            if (config is WUndergroundApiConfig wuConfig)
            {
                Config = wuConfig;
            }
        }
    }
}
