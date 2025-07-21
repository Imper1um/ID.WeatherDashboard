using GeoTimeZone;
using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.SunriseSunset.Data;
using Microsoft.Extensions.Logging;

namespace ID.WeatherDashboard.SunriseSunset.Services
{
    public class SunriseSunsetService(IJsonQueryService jsonQueryService, ILogger<SunriseSunsetService>? logger = null) : BaseService(logger), ISunDataService
    {
        public const string DefaultServiceName = "SunriseSunset";
        public const string _serviceUrl = "https://api.sunrise-sunset.org/json";
        private IJsonQueryService JsonQueryService { get; } = jsonQueryService;

        protected override string BaseServiceName => DefaultServiceName;

        public async Task<SunData?> GetSunDataAsync(Location location, DateTimeOffset from, DateTimeOffset to)
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
                throw new InvalidOperationException($"ApiKey is not provided when it is required to run this service.");

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
