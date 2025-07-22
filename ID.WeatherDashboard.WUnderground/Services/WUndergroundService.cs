using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.WUnderground.Data;
using Microsoft.Extensions.Logging;

namespace ID.WeatherDashboard.WUnderground.Services
{
    /// <summary>
    ///     Provides access to the Weather Underground API.
    /// </summary>
    /// <param name="jsonQueryService">Service used to perform HTTP queries.</param>
    /// <param name="logger">Optional logger instance.</param>
    public class WUndergroundService(IJsonQueryService jsonQueryService, ILogger<WUndergroundService>? logger = null) : BaseKeyedService<WUndergroundApiConfig>(logger), ICurrentQueryService, IHistoryQueryService
    {
        /// <summary>The default service name.</summary>
        public const string DefaultServiceName = "WUnderground";
        /// <summary>The base URL for history queries.</summary>
        public const string _historyUrl = "https://api.weather.com/v2/pws/history/all";
        /// <summary>The base URL for current observations.</summary>
        public const string _currentUrl = "https://api.weather.com/v2/pws/observations/current";

        /// <summary>
        ///     Gets the station identifier from the configuration.
        /// </summary>
        public string StationId => Config?.StationId ?? string.Empty;

        /// <inheritdoc />
        protected override string BaseServiceName => DefaultServiceName;

        private IJsonQueryService JsonQueryService { get; } = jsonQueryService;

        /// <summary>
        ///     Retrieves the latest <see cref="CurrentData"/> for the configured station.
        /// </summary>
        /// <param name="location">The location is ignored but required by the interface.</param>
        /// <returns>The current conditions data.</returns>
        /// <exception cref="QueryFailureException">Thrown when the query fails.</exception>
        public async Task<CurrentData?> GetCurrentDataAsync(Location location)
        {
            var url = $"{_currentUrl}?stationId={StationId}&format=json&units=e&apiKey={ApiKey}&numericPrecision=decimal";
            return (await JsonQueryService.QueryAsync<Observations>(url))?.ToCurrentData();
        }

        /// <summary>
        ///     Retrieves history data for the given date range.
        /// </summary>
        /// <param name="location">The location of the station.</param>
        /// <param name="from">Start of the range.</param>
        /// <param name="to">End of the range.</param>
        /// <returns>The history data retrieved from the service.</returns>
        /// <exception cref="QueryFailureException">Thrown when the query fails.</exception>
        public async Task<HistoryData?> GetHistoryDataAsync(Location location, DateTimeOffset from, DateTimeOffset to)
        {
            var url = $"{_historyUrl}?stationId={StationId}&format=json&units=e&apiKey={ApiKey}&numericPrecision=decimal&startDate={from:yyyyMMdd}&endDate={to:yyyyMMdd}";
            return (await JsonQueryService.QueryAsync<Observations>(url))?.ToHistoryData();
        }
    }
}
