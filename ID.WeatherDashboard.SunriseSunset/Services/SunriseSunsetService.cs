using GeoTimeZone;
using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.SunriseSunset.Data;
using Microsoft.Extensions.Logging;

namespace ID.WeatherDashboard.SunriseSunset.Services
{
    /// <summary>
    ///     Service that retrieves sunrise and sunset information from the Sunrise-Sunset API.
    /// </summary>
    /// <param name="jsonQueryService">Service used to perform HTTP queries.</param>
    /// <param name="logger">Optional logger instance.</param>
    public class SunriseSunsetService(IJsonQueryService jsonQueryService, ILogger<SunriseSunsetService>? logger = null) : BaseService<SunriseSunsetApiConfig>(logger), ISunDataService
    {
        /// <summary>The default service name.</summary>
        public const string DefaultServiceName = "SunriseSunset";
        /// <summary>The API base URL.</summary>
        public const string _serviceUrl = "https://api.sunrise-sunset.org/json";
        private IJsonQueryService JsonQueryService { get; } = jsonQueryService;

        /// <inheritdoc />
        protected override string BaseServiceName => DefaultServiceName;

        /// <summary>
        ///     Retrieves <see cref="SunData"/> information for the specified location.
        /// </summary>
        /// <param name="location">The location to query.</param>
        /// <param name="from">The start date.</param>
        /// <param name="to">The end date.</param>
        /// <returns>The retrieved <see cref="SunData"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="location"/> does not contain latitude or longitude.</exception>
        /// <exception cref="QueryFailureException">Thrown when the query fails.</exception>
        public async Task<SunData?> GetSunDataAsync(Location location, DateTimeOffset from, DateTimeOffset to)
        {
            var sunLines = new List<SunLine>();
            for (var dt = from.Date; dt <= to.Date; dt = dt.AddDays(1))
            {
                if (!TryCall()) continue;
                var lat = location.Latitude?.ToString("F6");
                var lng = location.Longitude?.ToString("F6");
                if (lat == null || lng == null)
                {
                    throw new ArgumentException("Location must have valid latitude and longitude.");
                }
                var tzid = TimeZoneLookup.GetTimeZone(location.Latitude!.Value, location.Longitude!.Value).Result;
                var url = $"{_serviceUrl}?lat={lat}&lng={lng}&formatted=0&date={from:yyyy-MM-dd}&timezone={tzid}";

                var sunLine = (await JsonQueryService.QueryAsync<SunriseSunsetApiResult>(url))?.ToSunLine(location.Latitude);
                if (sunLine != null)
                    sunLines.Add(sunLine);
            }
            return new SunData(DateTimeOffset.Now, [.. sunLines]);
        }
    }
}
