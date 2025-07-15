using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.WeatherAPI.Data
{
    public class WeatherApiDay
    {
        [JsonPropertyName("maxtemp_c")]
        public float? MaximumTemperatureCelsius { get; set; }
        [JsonPropertyName("maxtemp_f")]
        public float? MaximumTemperatureFahrenheit { get; set; }
        [JsonPropertyName("mintemp_c")]
        public float? MinimumTemperatureCelsius { get; set; }
        [JsonPropertyName("mintemp_f")]
        public float? MinimumTemperatureFahrenheit { get; set; }
        [JsonPropertyName("avgtemp_c")]
        public float? AverageTemperatureCelsius { get; set; }
        [JsonPropertyName("avgtemp_f")]
        public float? AverageTemperatureFahrenheit { get; set; }
        [JsonPropertyName("maxwind_mph")]
        public float? MaximumWindMilesPerHour { get; set; }
        [JsonPropertyName("maxwind_kph")]
        public float? MaximumWindKilometersPerHour { get; set; }
        [JsonPropertyName("totalprecip_in")]
        public float? TotalPrecipitationInches { get; set; }
        [JsonPropertyName("totalprecip_mm")]
        public float? TotalPrecipitationMillimeters { get; set; }
        [JsonPropertyName("totalsnow_cm")]
        public float? TotalSnowCentimeters { get; set; }
        [JsonPropertyName("avgvis_km")]
        public float? AverageVisibilityKilometers { get; set; }
        [JsonPropertyName("avgvis_miles")]
        public float? AverageVisibilityMiles { get; set; }
        [JsonPropertyName("avghumidity")]
        public float? AverageHumidity { get; set; }
        [JsonPropertyName("daily_will_it_rain")]
        public int? DailyWillItRain { get; set; }
        [JsonPropertyName("daily_chance_of_rain")]
        public float? DailyChanceOfRain { get; set; }
        [JsonPropertyName("daily_will_it_snow")]
        public int? DailyWillItSnow { get; set; }
        [JsonPropertyName("daily_chance_of_snow")]
        public float? DailyChanceOfSnow { get; set; }
        [JsonPropertyName("condition")]
        public WeatherApiCondition? Condition { get; set; }
        [JsonPropertyName("uv")]
        public float? UvIndex { get; set; }
    }
}
