using ID.WeatherDashboard.API.Codes;
using ID.WeatherDashboard.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.WeatherAPI.Data
{
    public class WeatherApiCurrentAPI
    {
        [JsonPropertyName("location")]
        public WeatherApiLocation? Location { get; set; }

        [JsonPropertyName("current")]
        public WeatherApiCurrent? Current { get; set; }

        public CurrentData? ToCurrentData()
        {
            if (Location == null || Current == null)
            {
                return null;
            }
            var pulled = DateTimeOffset.Now;
            var currentData = new CurrentData(pulled, "WeatherAPI")
            {
                Observed = Current?.LastUpdatedEpoch == null ? null : DateTimeOffset.FromUnixTimeSeconds(Current.LastUpdatedEpoch.Value),
                WindDirection = Current?.WindDegree == null ? null : new WindDirection(Current.WindDegree.Value),
                Humidity = Current?.Humidity,
                CurrentTemperature = new Temperature(Current?.TempFahrenheit),
                FeelsLike = new Temperature(Current?.FeelsLikeFahrenheit),
                HeatIndex = new Temperature(Current?.HeatIndexFahrenheit),
                DewPoint = new Temperature(Current?.DewpointFahrenheit),
                UVIndex = Current?.UvIndex,
                Pressure = new Pressure(Current?.PressureInches, PressureEnum.InchesOfMercury),
                Coordinates = Location?.ToCoordinates(),
                WeatherConditions = new WeatherConditions(Current?.LastUpdatedEpoch == null ? null : DateTimeOffset.FromUnixTimeSeconds(Current.LastUpdatedEpoch.Value))
                {
                    RainRate = Current?.Condition?.IsRain == true ? new Precipitation(Current?.PrecipitationInches, PrecipitationEnum.Inches) : null,
                    SnowRate = Current?.Condition?.IsSnow == true ? new Precipitation(Current?.PrecipitationInches, PrecipitationEnum.Inches) : null,
                    CloudCoverPercentage = Current?.CloudCoverPercentage,
                    WindGustSpeed = new WindSpeed(Current?.GustMilesPerHour),
                    WindSpeed = new WindSpeed(Current?.WindMph),
                    Visibility = new Distance(Current?.VisibilityMiles, DistanceEnum.Miles),
                    IsLightning = Current?.Condition?.IsLightning,
                    IsFoggy = Current?.Condition?.IsFoggy,
                    IsFreezing = Current?.TempFahrenheit <= 32,
                    IsHail = Current?.Condition?.IsHail,
                    IsSleet = Current?.Condition?.IsSleet,
                    IsRain = Current?.Condition?.IsRain,
                    StateConditions = Current?.Condition?.Text,
                }
            };
            return currentData;
        }
    }
}
