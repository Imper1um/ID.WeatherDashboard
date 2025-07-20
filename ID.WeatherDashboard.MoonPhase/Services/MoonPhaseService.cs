using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.MoonPhase.Data;
using System.Net.Http;
using System.Text.Json;

namespace ID.WeatherDashboard.MoonPhase.Services
{
    public class MoonPhaseService : BaseKeyedService<MoonPhaseConfig>, ISunDataService
    {
        public const string _ServiceName = "MoonPhase";
        public const string _advancedUrl = "https://moon-phase.p.rapidapi.com/advanced";

        private readonly HttpClient _httpClient;

        /// <summary>
        /// Gets or sets the base url used for API requests.  This can be overridden in
        /// unit tests to avoid hitting the real service.
        /// </summary>
        public string AdvancedUrl { get; set; } = _advancedUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoonPhaseService"/> class using a
        /// new <see cref="HttpClient"/>.
        /// </summary>
        public MoonPhaseService() : this(new HttpClient())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoonPhaseService"/> class with the
        /// specified <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> instance to use.</param>
        public MoonPhaseService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

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
            for (var d = from; d <= to; d = d.AddDays(1))
            {
                if (!TryCall())
                    continue;

                var url = $"{AdvancedUrl}?lat={lat}&lon={lon}&date={d:yyyy-MM-dd}";
                _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", "moon-phase.p.rapidapi.com");
                _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", ApiKey);
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStreamAsync();
                    var apiData = await JsonSerializer.DeserializeAsync<MoonPhaseAdvancedAPI>(content);
                    var sunData = apiData?.ToSunData(d.Date);
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
