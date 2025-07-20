using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.WUnderground.Data;
using Microsoft.Extensions.Logging;

namespace ID.WeatherDashboard.WUnderground.Services
{
    public class WUndergroundService(IJsonQueryService jsonQueryService, ILogger<WUndergroundService>? logger = null) : BaseKeyedService<WUndergroundApiConfig>(logger), ICurrentQueryService, IHistoryQueryService
    {
        public const string DefaultServiceName = "WUnderground";
        public const string _historyUrl = "https://api.weather.com/v2/pws/history/all";
        public const string _currentUrl = "https://api.weather.com/v2/pws/observations/current";

        public string StationId => Config?.StationId ?? string.Empty;

        protected override string BaseServiceName => DefaultServiceName;

        private IJsonQueryService JsonQueryService { get; } = jsonQueryService;

        public async Task<CurrentData?> GetCurrentDataAsync(Location location)
        {
            var url = $"{_currentUrl}?stationId={StationId}&format=json&units=e&apiKey={ApiKey}&numericPrecision=decimal";
            return (await JsonQueryService.QueryAsync<Observations>(url))?.ToCurrentData();
        }

        public async Task<HistoryData?> GetHistoryDataAsync(Location location, DateTimeOffset from, DateTimeOffset to)
        {
            var url = $"{_historyUrl}?stationId={StationId}&format=json&units=e&apiKey={ApiKey}&numericPrecision=decimal&startDate={from:yyyyMMdd}&endDate={to:yyyyMMdd}";
            return (await JsonQueryService.QueryAsync<Observations>(url))?.ToHistoryData();
        }
    }
}
