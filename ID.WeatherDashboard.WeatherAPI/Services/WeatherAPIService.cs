using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.WeatherAPI.Data;
using System.Text.Json;

namespace ID.WeatherDashboard.WeatherAPI.Services
{
    public class WeatherAPIService : BaseKeyedService<WeatherApiConfig>, IForecastQueryService, ICurrentQueryService, IHistoryQueryService, ISunDataService, IAlertQueryService
    {
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
            using var httpClient = new HttpClient();
            var q = location.ToString();
            var url = $"{_alertUrl}?key={ApiKey}&q={q}";
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                var alertData = await JsonSerializer.DeserializeAsync<WeatherApiAlertAPI>(stream);
                return alertData?.ToAlertData();
            }
            return null;
        }

        public async Task<CurrentData?> GetCurrentDataAsync(Location location)
        {
            if (!TryCall()) return null;
            using var httpClient = new HttpClient();
            var q = location.ToString();
            var url = $"{_currentUrl}?key={ApiKey}&q={q}";
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var currentData = JsonSerializer.Deserialize<WeatherApiCurrentAPI>(content);
                return currentData?.ToCurrentData();
            }
            return null;
        }

        public async Task<ForecastData?> GetForecastDataAsync(Location location, DateTimeOffset from, DateTimeOffset to)
        {
            if (!TryCall()) return null;
            using var httpClient = new HttpClient();
            var unixdt = from.ToUnixTimeSeconds();
            var days = (to - from).Days + 1;
            var q = location.ToString();
            var url = $"{_forecastUrl}?key={ApiKey}&q={q}&dt={unixdt}&days={days}";
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var forecastData = JsonSerializer.Deserialize<WeatherApiForecastAPI>(content);
                return forecastData?.ToForecastData();
            }
            return null;
        }

        public async Task<HistoryData?> GetHistoryDataAsync(Location location, DateTimeOffset from, DateTimeOffset to)
        {
            if (!TryCall()) return null;
            if (to.Subtract(from).TotalDays > 30)
            {
                throw new ArgumentException("WeatherAPI only supports history data for up to 30 days.");
            }

            using var httpClient = new HttpClient();
            var q = location.ToString();
            var unixdt = from.ToUnixTimeSeconds();
            var unixend_dt = to.ToUnixTimeSeconds();

            var url = $"{_historyUrl}?key={ApiKey}&q={q}&dt={unixdt}&end_dt={unixend_dt}";
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var historyData = JsonSerializer.Deserialize<WeatherApiHistoryAPI>(content);
                return historyData?.ToHistoryData();
            }
            return null;
        }

        public async Task<SunData?> GetSunDataAsync(Location location, DateTime from, DateTime to)
        {
            var sunDatas = new List<SunData>();
            for (var d = from; d <= to; d = d.AddDays(1))
            {
                if (!TryCall()) continue;
                using var httpClient = new HttpClient();
                var q = location.ToString();
                var url = $"{_astronomyUrl}?key={ApiKey}&q={q}&dt={d:yyyy-MM-dd}";
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var astronomyData = JsonSerializer.Deserialize<WeatherApiAstronomyAPI>(content);
                    var sunData = astronomyData?.ToSunData();
                    if (sunData != null)
                    {
                        sunDatas.Add(sunData);
                    }
                }
            }
            return new SunData(DateTimeOffset.Now, sunDatas.SelectMany(sd => sd.Lines).ToArray());
        }

        public Task<SunData?> GetSunDataAsync(Location location, DateTimeOffset from, DateTimeOffset to)
        {
            throw new NotImplementedException();
        }
    }
}
