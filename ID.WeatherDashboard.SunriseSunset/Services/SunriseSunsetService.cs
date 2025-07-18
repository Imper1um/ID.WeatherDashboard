using GeoTimeZone;
using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.SunriseSunset.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.SunriseSunset.Services
{
    public class SunriseSunsetService : ISunDataService
    {
        public const string DefaultServiceName = "SunriseSunset";
        public string ServiceName => Config?.Name ?? DefaultServiceName;

        public const string _serviceUrl = "https://api.sunrise-sunset.org/json";
        public SunriseSunsetApiConfig? Config { get; set; }

        public async Task<SunData?> GetSunDataAsync(Location location, DateTimeOffset from, DateTimeOffset to)
        {
            var sunLines = new List<SunLine>();
            for (var dt = from.Date; dt <= to.Date; dt = dt.AddDays(1))
            {
                using var httpClient = new HttpClient();
                var lat = location.Latitude?.ToString("F6");
                var lng = location.Longitude?.ToString("F6");
                if (lat == null || lng == null)
                {
                    throw new ArgumentException("Location must have valid latitude and longitude.");
                }
                var tzid = TimeZoneLookup.GetTimeZone(location.Latitude!.Value, location.Longitude!.Value).Result;
                var url = $"{_serviceUrl}?lat={lat}&lng={lng}&formatted=0&date={from:yyyy-MM-dd}&timezone={tzid}";
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = await JsonSerializer.DeserializeAsync<SunriseSunsetApiResult>(await response.Content.ReadAsStreamAsync());
                    if (result != null)
                    {
                        sunLines.Add(result.ToSunLine(location.Latitude));
                    }
                }
            }
            return new SunData(DateTimeOffset.Now, sunLines.ToArray());
        }

        public void SetServiceConfig(ServiceConfig config)
        {
            if (config is SunriseSunsetApiConfig sunriseSunsetConfig)
            {
                Config = sunriseSunsetConfig;
            }
        }
    }
}
