using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.MoonPhase.Data;
using System.Text.Json;

namespace ID.WeatherDashboard.MoonPhase.Services
{
    public class MoonPhaseService : BaseKeyedService<MoonPhaseConfig>, ISunDataService
    {
        public const string _ServiceName = "MoonPhase";
        public const string _advancedUrl = "https://moon-phase.p.rapidapi.com/advanced";

        protected override string BaseServiceName => _ServiceName;

        public async Task<SunData?> GetSunDataAsync(Location location, DateTime from, DateTime to)
        {
            var lat = location.Latitude?.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
            var lon = location.Longitude?.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
            if (lat == null || lon == null)
            {
                return null; // Invalid location
            }
            var sunDatas = new List<SunData>();
            for (var d = from; d <= to; d = d.AddDays(1))
            {
                if (!TryCall()) continue;
                using var httpClient = new HttpClient();
                var url = $"{_advancedUrl}?lat={lat}&lon={lon}&date={d:yyyy-MM-dd}";
                httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", "moon-phase.p.rapidapi.com");
                httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", ApiKey);
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStreamAsync();
                    var apiData = await JsonSerializer.DeserializeAsync<MoonPhaseAdvancedAPI>(content);
                    var sunData = apiData?.ToSunData(d);
                    if (sunData != null)
                    {
                        sunDatas.Add(sunData);
                    }
                }
            }
            return new SunData(DateTimeOffset.Now, sunDatas.SelectMany(sd => sd.Lines).ToArray());
        }
    }
}
