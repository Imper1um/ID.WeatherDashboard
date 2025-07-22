using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.WeatherAPI.Data;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ID.WeatherDashboard.WeatherAPI.Services
{
    /// <summary>
    ///     Service providing access to the WeatherAPI.com endpoints.
    /// </summary>
    /// <param name="jsonQueryService">Service used to perform HTTP queries.</param>
    /// <param name="logger">Optional logger instance.</param>
    public class WeatherAPIService(IJsonQueryService jsonQueryService, ILogger<WeatherAPIService>? logger = null) : BaseKeyedService<WeatherApiConfig>(logger), IForecastQueryService, ICurrentQueryService, IHistoryQueryService, ISunDataService, IAlertQueryService
    {
        private IJsonQueryService JsonQueryService { get; } = jsonQueryService;
        /// <summary>The default service name.</summary>
        public const string _ServiceName = "WeatherAPI";
        /// <summary>URL for current conditions.</summary>
        public const string _currentUrl = "https://api.weatherapi.com/v1/current.json";
        /// <summary>URL for history queries.</summary>
        public const string _historyUrl = "https://api.weatherapi.com/v1/history.json";
        /// <summary>URL for forecast queries.</summary>
        public const string _forecastUrl = "https://api.weatherapi.com/v1/forecast.json";
        /// <summary>URL for astronomy information.</summary>
        public const string _astronomyUrl = "https://api.weatherapi.com/v1/astronomy.json";
        /// <summary>URL for weather alerts.</summary>
        public const string _alertUrl = "https://api.weatherapi.com/v1/alerts.json";

        /// <inheritdoc />
        protected override string BaseServiceName => _ServiceName;

        /// <summary>
        ///     Retrieves weather alerts for the specified <paramref name="location"/>.
        /// </summary>
        /// <param name="location">The location to query.</param>
        /// <returns>The alert data if available.</returns>
        /// <exception cref="QueryFailureException">Thrown when the query fails.</exception>
        public async Task<AlertData?> GetAlertDataAsync(Location location)
        {
            if (!TryCall()) return null;
            var q = location.ToString();
            var url = $"{_alertUrl}?key={ApiKey}&q={q}";
            return (await JsonQueryService.QueryAsync<WeatherApiAlertAPI>(url))?.ToAlertData();
        }

        /// <summary>
        ///     Retrieves the current weather conditions for the specified location.
        /// </summary>
        /// <param name="location">The location to query.</param>
        /// <returns>The current conditions data.</returns>
        /// <exception cref="QueryFailureException">Thrown when the query fails.</exception>
        public async Task<CurrentData?> GetCurrentDataAsync(Location location)
        {
            if (!TryCall()) return null;
            var q = location.ToString();
            var url = $"{_currentUrl}?key={ApiKey}&q={q}";
            return (await JsonQueryService.QueryAsync<WeatherApiCurrentAPI>(url))?.ToCurrentData();
        }

        /// <summary>
        ///     Retrieves forecast data for the specified time range.
        /// </summary>
        /// <param name="location">The location to query.</param>
        /// <param name="from">Start of the forecast period.</param>
        /// <param name="to">End of the forecast period.</param>
        /// <returns>The forecast data.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="to"/> is more than 14 days from <paramref name="from"/>.</exception>
        /// <exception cref="QueryFailureException">Thrown when the query fails.</exception>
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

        /// <summary>
        ///     Retrieves historical weather data for the specified date range.
        /// </summary>
        /// <param name="location">The location to query.</param>
        /// <param name="from">Start of the range.</param>
        /// <param name="to">End of the range.</param>
        /// <returns>The historical data.</returns>
        /// <exception cref="ArgumentException">Thrown when the range exceeds 30 days.</exception>
        /// <exception cref="QueryFailureException">Thrown when the query fails.</exception>
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

        /// <summary>
        ///     Retrieves sun and moon information for a date range.
        /// </summary>
        /// <param name="location">The location to query.</param>
        /// <param name="from">Start date.</param>
        /// <param name="to">End date.</param>
        /// <returns>The combined <see cref="SunData"/>.</returns>
        /// <exception cref="QueryFailureException">Thrown when a query fails.</exception>
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
