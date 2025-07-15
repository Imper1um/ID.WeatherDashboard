using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.WeatherAPI.Data
{
    public class WeatherApiCurrent
    {
        [JsonPropertyName("last_updated_epoch")]
        public long? LastUpdatedEpoch { get; set; }
        [JsonPropertyName("last_updated")]
        public string? LastUpdated { get; set; }
        [JsonPropertyName("temp_c")]
        public float? TempCelsius { get; set; }
        [JsonPropertyName("temp_f")]
        public float? TempFahrenheit { get; set; }
        [JsonPropertyName("is_day")]
        public short? IsDay { get; set; }
        [JsonPropertyName("condition")]
        public WeatherApiCondition? Condition { get; set; }
        [JsonPropertyName("wind_mph")]
        public float? WindMph { get; set; }
        [JsonPropertyName("wind_kph")]
        public float? WindKph { get; set; }
        [JsonPropertyName("wind_degree")]
        public int? WindDegree { get; set; }
        [JsonPropertyName("wind_dir")]
        public string? WindDirection { get; set; }
        [JsonPropertyName("pressure_mb")]
        public float? PressureMb { get; set; }
        [JsonPropertyName("pressure_in")]
        public float? PressureInches { get; set; }
        [JsonPropertyName("precip_mm")]
        public float? PrecipitationMm { get; set; }
        [JsonPropertyName("precip_in")]
        public float? PrecipitationInches { get; set; }
        [JsonPropertyName("humidity")]
        public float? Humidity { get; set; }
        [JsonPropertyName("cloud")]
        public float? CloudCoverPercentage { get; set; }
        [JsonPropertyName("feelslike_c")]
        public float? FeelsLikeCelsius { get; set; }
        [JsonPropertyName("feelslike_f")]
        public float? FeelsLikeFahrenheit { get; set; }
        [JsonPropertyName("windchill_c")]
        public float? WindChillCelsius { get; set; }
        [JsonPropertyName("windchill_f")]
        public float? WindChillFahrenheit { get; set; }
        [JsonPropertyName("heatindex_c")]
        public float? HeatIndexCelsius { get; set; }
        [JsonPropertyName("heatindex_f")]
        public float? HeatIndexFahrenheit { get; set; }
        [JsonPropertyName("dewpoint_c")]
        public float? DewpointCelsius { get; set; }
        [JsonPropertyName("dewpoint_f")]
        public float? DewpointFahrenheit { get; set; }
        [JsonPropertyName("vis_km")]
        public float? VisibilityKilometers { get; set; }
        [JsonPropertyName("vis_miles")]
        public float? VisibilityMiles { get; set; }
        [JsonPropertyName("uv")]
        public float? UvIndex { get; set; }
        [JsonPropertyName("gust_mph")]
        public float? GustMilesPerHour { get; set; }
        [JsonPropertyName("gust_kph")]
        public float? GustKilometersPerHour { get; set; }
    }
}
