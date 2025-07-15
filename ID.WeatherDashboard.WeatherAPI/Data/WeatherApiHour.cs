using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Codes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.WeatherAPI.Data
{
    public class WeatherApiHour
    {
        public DateTimeOffset Pulled { get; set; } = DateTimeOffset.Now;

        [JsonPropertyName("time_epoch")]
        public double? LastUpdatedEpoch { get; set; }
        [JsonPropertyName("time")]
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
        public string? WindDir { get; set; }
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
        [JsonPropertyName("will_it_rain")]
        public int? WillItRain { get; set; }
        [JsonPropertyName("will_it_snow")]
        public int? WillItSnow { get; set; }
        [JsonPropertyName("chance_of_rain")]
        public float? ChanceOfRain { get; set; }
        [JsonPropertyName("chance_of_snow")]
        public float? ChanceOfSnow { get; set; }

        public WeatherConditions ToWeatherConditions()
        {
            return new WeatherConditions(DateTimeOffset.FromUnixTimeSeconds((long)(LastUpdatedEpoch ?? 0)))
            {
                RainRate = new Precipitation(PrecipitationInches),
                CloudCoverPercentage = CloudCoverPercentage,
                WindGustSpeed = new WindSpeed(GustMilesPerHour),
                WindSpeed = new WindSpeed(WindMph),
                Visibility = new Distance(VisibilityMiles),
                IsLightning = Condition?.IsLightning,
                IsFoggy = Condition?.IsFoggy,
                IsHail = Condition?.IsHail,
                IsRain = Condition?.IsRain,
                IsSleet = Condition?.IsSleet,
                IsFreezing = TempFahrenheit <= 32,
                StateConditions = Condition?.Text
            };
        }

        public HistoryLine ToHistoryLine()
        {
            var historyLine = new HistoryLine(Pulled, "WeatherAPI")
            {
                Observed = DateTimeOffset.FromUnixTimeSeconds((long)(LastUpdatedEpoch ?? 0)),
                WindDirection = WindDegree == null ? null : new WindDirection(WindDegree.Value),
                Humidity = Humidity,
                CurrentTemperature = new Temperature(TempFahrenheit),
                FeelsLike = new Temperature(FeelsLikeFahrenheit),
                HeatIndex = new Temperature(HeatIndexFahrenheit),
                DewPoint = new Temperature(DewpointFahrenheit),
                UVIndex = UvIndex,
                Pressure = new Pressure(PressureMb),
                WeatherConditions = ToWeatherConditions()
            };
            return historyLine;
        }

        public ForecastLine ToForecastLine()
        {
            var forecastLine = new ForecastLine(Pulled, "WeatherAPI")
            {
                Observed = DateTimeOffset.FromUnixTimeSeconds((long)(LastUpdatedEpoch ?? 0)),
                WindDirection = WindDegree == null ? null : new WindDirection(WindDegree.Value),
                Humidity = Humidity,
                CurrentTemperature = new Temperature(TempFahrenheit),
                FeelsLike = new Temperature(FeelsLikeFahrenheit),
                HeatIndex = new Temperature(HeatIndexFahrenheit),
                DewPoint = new Temperature(DewpointFahrenheit),
                UVIndex = UvIndex,
                Pressure = new Pressure(PressureMb),
                RainChance = ChanceOfRain,
                SnowChance = ChanceOfSnow,
                WeatherConditions = ToWeatherConditions()
            };
            return forecastLine;
        }
    }
}
