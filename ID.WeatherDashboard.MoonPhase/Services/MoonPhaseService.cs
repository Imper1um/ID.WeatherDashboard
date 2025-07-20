using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.MoonPhase.Data;
using Microsoft.Extensions.Logging;

namespace ID.WeatherDashboard.MoonPhase.Services
{
    public class MoonPhaseService(IJsonQueryService jsonQueryService, ILogger<MoonPhaseService>? logger = null) : BaseKeyedService<MoonPhaseConfig>(logger), ISunDataService
    {
        private IJsonQueryService JsonQueryService { get; } = jsonQueryService;
        public const string _ServiceName = "MoonPhase";
        public const string _advancedUrl = "https://moon-phase.p.rapidapi.com/advanced";

        protected override string BaseServiceName => _ServiceName;

        public async Task<SunData?> GetSunDataAsync(Location location, DateTimeOffset from, DateTimeOffset to)
        {
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
                    var sunData = (await JsonQueryService.QueryAsync<MoonPhaseAdvancedAPI>(url, [.. headers]))?.ToSunData(d.Date);
                    if (sunData != null)
                        sunDatas.Add(sunData);
                } 
                catch (Exception ex)
                {
                    //TODO: Log this.
                }
            }
            return new SunData(DateTimeOffset.Now, [.. sunDatas.SelectMany(sd => sd.Lines)]);
        }
    }
}
