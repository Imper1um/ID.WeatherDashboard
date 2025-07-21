using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.MoonPhase.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ID.WeatherDashboard.MoonPhase.Services
{
    public class MoonPhaseService(IJsonQueryService jsonQueryService, ILocationStorageService locationStorageService, ILogger<MoonPhaseService>? logger = null) : BaseKeyedService<MoonPhaseConfig>(logger), ISunDataService
    {
        private IJsonQueryService JsonQueryService { get; } = jsonQueryService;
        private ILocationStorageService LocationStorageService { get; } = locationStorageService;
        public const string _ServiceName = "MoonPhase";
        public const string _advancedUrl = "https://moon-phase.p.rapidapi.com/advanced";

        protected override string BaseServiceName => _ServiceName;

        public async Task<SunData?> GetSunDataAsync(Location location, DateTimeOffset from, DateTimeOffset to)
        {
            if (location.Latitude == null || location.Longitude == null)
                LocationStorageService.ResolveGeolocation(location);
            var lat = location.Latitude?.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
            var lon = location.Longitude?.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
            if (lat == null || lon == null)
            {
                return null; // Invalid location
            }
            var sunDatas = new List<SunData>();
            var headers = new List<Tuple<string, string>>()
            {
                new("x-rapidapi-host", "moon-phase.p.rapidapi.com"),
                new("x-rapidapi-key", ApiKey)
            };
            for (var d = from; d <= to; d = d.AddDays(1))
            {
                if (!TryCall()) continue;
                var url = $"{_advancedUrl}?lat={lat}&lon={lon}&date={d:yyyy-MM-dd}";

                try
                {
                    var result = await JsonQueryService.QueryAsync<MoonPhaseAdvancedAPI>(url, [.. headers]);
                    var sunData = result?.ToSunData(d.Date);
                    if (sunData != null)
                    {
                        if (!string.IsNullOrWhiteSpace(location.Name))
                            LocationStorageService.UploadGeolocation(new Location(location.Name) { Latitude = result!.Location!.Latitude, Longitude = result.Location.Longitude });
                        sunDatas.Add(sunData);
                    }
                } 
                catch (Exception ex)
                {
                    Logger.LogDebug(ex, "Exception caused when trying to parse MoonPhase Data: {ExceptionData}", ex.GetFullMessage());
                }
            }
            return new SunData(DateTimeOffset.Now, [.. sunDatas.SelectMany(sd => sd.Lines)]);
        }
    }
}
