using ID.WeatherDashboard.API.Data;
using System.Text.Json.Serialization;

namespace ID.WeatherDashboard.WeatherAPI.Data
{
    public class WeatherApiForecastDay
    {
        public DateTimeOffset Pulled { get; set; } = DateTimeOffset.Now;

        [JsonPropertyName("date")]
        public string? Date { get; set; }
        [JsonPropertyName("date_epoch")]
        public long? DateEpoch { get; set; }
        [JsonPropertyName("day")]
        public WeatherApiDay? Day { get; set; }
        [JsonPropertyName("astro")]
        public WeatherApiAstro? Astro { get; set; }
        [JsonPropertyName("hour")]
        public List<WeatherApiHour>? Hours { get; set; }

        public ForecastDay ToForecastDay()
        {
            var forecastDay = new ForecastDay(DateTimeOffset.FromUnixTimeSeconds(DateEpoch ?? 0), "WeatherAPI");
            if (Day != null)
            {
                forecastDay.Daytime = new DaytimeData
                {
                    High = new Temperature(Day?.MaximumTemperatureFahrenheit),
                    Rain = new Precipitation(Day?.TotalPrecipitationInches, API.Codes.PrecipitationEnum.Inches),
                    RainPercentage = Day?.DailyChanceOfRain,
                    Snow = new Precipitation(Day?.TotalSnowCentimeters, API.Codes.PrecipitationEnum.Centimeters),
                    SnowPercentage = Day?.DailyChanceOfSnow,
                    ForecastText = Day?.Condition?.Text
                };
                forecastDay.Nighttime = new NighttimeData
                {
                    Low = new Temperature(Day?.MinimumTemperatureFahrenheit)
                };
            }
            if (Astro != null)
            {
                forecastDay.MoonPhase = Astro.MoonPhase?.ToMoonPhase();
            }
            if (Hours != null)
            {
                foreach (var hour in Hours)
                {
                    var fl = hour.ToForecastLine();
                    if (fl.WeatherConditions != null)
                        fl.WeatherConditions.MoonPhase = forecastDay.MoonPhase;
                    forecastDay.AddLine(fl);
                }
            }
            return forecastDay;
        }
    }
}
