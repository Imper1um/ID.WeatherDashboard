using System.Text.Json.Serialization;

namespace ID.WeatherDashboard.MoonPhase.Data
{
    public class MoonPhaseZodiac
    {
        [JsonPropertyName("sun_sign")]
        public string? SunSign { get; set; }
        [JsonPropertyName("moon_sign")]
        public string? MoonSign { get; set; }
    }
}
