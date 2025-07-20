using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.WeatherAPI.Data;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ID.WeatherDashboard.WeatherAPI.Services
{
    public class WeatherAPIService(IJsonQueryService jsonQueryService, ILogger<WeatherAPIService>? logger = null) : BaseKeyedService<WeatherApiConfig>(logger), IForecastQueryService, ICurrentQueryService, IHistoryQueryService, ISunDataService, IAlertQueryService
    {
        private IJsonQueryService JsonQueryService { get; } = jsonQueryService;
        public const string _ServiceName = "WeatherAPI";
        public const string _currentUrl = "https://api.weatherapi.com/v1/current.json";
        public const string _historyUrl = "https://api.weatherapi.com/v1/history.json";
        public const string _forecastUrl = "https://api.weatherapi.com/v1/forecast.json";
        public const string _astronomyUrl = "https://api.weatherapi.com/v1/astronomy.json";
        public const string _alertUrl = "https://api.weatherapi.com/v1/alerts.json";

        protected override string BaseServiceName => _ServiceName;

        public async Task<AlertData?> GetAlertDataAsync(Location location)
        {
            if (!TryCall()) return null;
            var q = location.ToString();
            var url = $"{_alertUrl}?key={ApiKey}&q={q}";
            return (await JsonQueryService.QueryAsync<WeatherApiAlertAPI>(url))?.ToAlertData();
        }

        public async Task<CurrentData?> GetCurrentDataAsync(Location location)
        {
            if (!TryCall()) return null;
            var q = location.ToString();
            var url = $"{_currentUrl}?key={ApiKey}&q={q}";
            return (await JsonQueryService.QueryAsync<WeatherApiCurrentAPI>(url))?.ToCurrentData();
        }

        public async Task<ForecastData?> GetForecastDataAsync(Location location, DateTimeOffset from, DateTimeOffset to)
        {
            if (!TryCall()) return null;
            var unixdt = from.ToUnixTimeSeconds();
            var days = (to - from).Days + 1;
            if (days > 14)
            {
                throw new ArgumentException("WeatherAPI only supports forecast data for up to 14 days in the future.", nameof(to));
            }
            var q = location.ToString();
            var url = $"{_forecastUrl}?key={ApiKey}&q={q}&dt={unixdt}&days={days}";
            return (await JsonQueryService.QueryAsync<WeatherApiForecastAPI>(url))?.ToForecastData();
        }

        public async Task<HistoryData?> GetHistoryDataAsync(Location location, DateTimeOffset from, DateTimeOffset to)
        {
            if (!TryCall()) return null;
            if (to.Subtract(from).TotalDays > 30)
            {
                throw new ArgumentException("WeatherAPI only supports history data for up to 30 days.", nameof(to));
            }

            var q = location.ToString();
            var unixdt = from.ToUnixTimeSeconds();
            var unixend_dt = to.ToUnixTimeSeconds();

            var url = $"{_historyUrl}?key={ApiKey}&q={q}&dt={unixdt}&end_dt={unixend_dt}";
            return (await JsonQueryService.QueryAsync<WeatherApiHistoryAPI>(url))?.ToHistoryData();
        }

        public async Task<SunData?> GetSunDataAsync(Location location, DateTimeOffset from, DateTimeOffset to)
        {
            var sunDatas = new List<SunData>();
            for (var d = from; d <= to; d = d.AddDays(1))
            {
                if (!TryCall()) continue;
                var q = location.ToString();
                var url = $"{_astronomyUrl}?key={ApiKey}&q={q}&dt={d:yyyy-MM-dd}";
                var sunData = (await JsonQueryService.QueryAsync<WeatherApiAstronomyAPI>(url))?.ToSunData();
                if (sunData != null) { sunDatas.Add(sunData); }
            }
            return new SunData(DateTimeOffset.Now, [.. sunDatas.SelectMany(sd => sd.Lines)]);
        }
    }
}
