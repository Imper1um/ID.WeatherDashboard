using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.WeatherAPI.Data
{
    public class WeatherApiCondition
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
        [JsonPropertyName("icon")]
        public string? Icon { get; set; }
        [JsonPropertyName("code")]
        public int? Code { get; set; }

        public static int[] LightningCodes = new[]
        {
            1087, // Thundery outbreaks in nearby
            1273, //Patchy light rain in area with thunder
            1276, //Moderate or heavy rain in area with thunder
            1279, //Patchy light snow in area with thunder
            1282, //Moderate or heavy snow in area with thunder
        };

        public bool IsLightning => LightningCodes.Contains(Code.GetValueOrDefault());

        public static int[] FoggyCodes = new[]
        {
            1135, // Fog
            1147, // Freezing fog
        };

        public bool IsFoggy => FoggyCodes.Contains(Code.GetValueOrDefault());

        public static int[] HailCodes = new[]
        {
            1237, //Ice pellets
            1261, //Light showers of ice pellets
            1264, //Moderate or heavy showers of ice pellets
        };

        public bool IsHail => HailCodes.Contains(Code.GetValueOrDefault());

        public static int[] SnowCodes = new[]
        {
            1066, // Patchy snow nearby
            1114, // Blowing snow
            1117, // Blizzard
            1207, //Moderate or heavy sleet
            1210, //Patchy light snow
            1213, //Light snow
            1216, //Patchy moderate snow
            1219, //Moderate snow
            1222, //Patchy heavy snow
            1225, //Heavy snow
            1258, //Moderate or heavy snow showers
        };

        public bool IsSnow => SnowCodes.Contains(Code.GetValueOrDefault());

        public static int[] RainCodes = new[]
        {
            1030, // Mist
            1063, // Patchy rain nearby
            1072, //Patchy freezing drizzle nearby
            1087, // Thundery outbreaks in nearby
            1150, // Patchy light drizzle
            1153, // Light drizzle
            1168, // Light freezing drizzle
            1171, // Heavy freezing drizzle
            1180, // Patchy light rain
            1183, // Light rain
            1186, // Moderate rain at times
            1189, // Moderate rain
            1192, // Heavy rain at times
            1195, // Heavy rain
            1198, // Light freezing rain
            1201, // Moderate or heavy freezing rain
            1240, // Light rain showers
            1243, // Moderate or heavy rain showers
            1246, // Heavy rain showers
            1273, // Patchy light rain in area with thunder
            1276, // Moderate or heavy rain in area with thunder
        };

        public bool IsRain => RainCodes.Contains(Code.GetValueOrDefault());

        public static int[] SleetCodes = new[]
        {
            1069, // Patchy sleet nearby
            1075, // Patchy light sleet
            1080, // Light sleet
            1083, // Moderate or heavy sleet
            1174, // Light sleet showers
            1181, // Moderate or heavy sleet showers
        };

        public bool IsSleet => SleetCodes.Contains(Code.GetValueOrDefault());
    }
}