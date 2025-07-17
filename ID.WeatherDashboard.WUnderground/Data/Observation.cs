using ID.WeatherDashboard.API.Codes;
using ID.WeatherDashboard.API.Data;
using System.Text.Json.Serialization;

namespace ID.WeatherDashboard.WUnderground.Data
{
    public class Observation
    {
        public DateTimeOffset Pulled { get; set; } = DateTimeOffset.Now;

        [JsonPropertyName("stationID")]
        public string? StationId { get; set; }
        [JsonPropertyName("obsTimeUtc")]
        public string? ObservationTimeUtc { get; set; }
        [JsonPropertyName("obsTimeLocal")]
        public string? ObservationTimeLocal { get; set; }
        [JsonPropertyName("neighborhood")]
        public string? Neighborhood { get; set; }
        [JsonPropertyName("softwareType")]
        public string? SoftwareType { get; set; }
        [JsonPropertyName("country")]
        public string? Country { get; set; }
        [JsonPropertyName("solarRadiation")]
        public float? SolarRadiation { get; set; }
        [JsonPropertyName("lat")]
        public double? Latitude { get; set; }
        [JsonPropertyName("lon")]
        public double? Longitude { get; set; }
        [JsonPropertyName("epoch")]
        public double? Epoch { get; set; }
        [JsonPropertyName("uv")]
        public float? Uv { get; set; }
        [JsonPropertyName("winddir")]
        public int? WindDirection { get; set; }
        [JsonPropertyName("humidity")]
        public float? Humidity { get; set; } //0.0 to 100.0
        [JsonPropertyName("qcStatus")]
        public int? QcStatus { get; set; }
        [JsonPropertyName("imperial")]
        public Imperial? Imperial { get; set; }

        public WeatherConditions? ToWeatherConditions()
        {
            if (Imperial == null)
                return null;
            var conditions = new WeatherConditions(Pulled)
            {
                BasePrecipitationRate = new Precipitation(Imperial.PrecipitationRate),
                WindGustSpeed = new WindSpeed(Imperial.WindGust),
                WindSpeed = new WindSpeed(Imperial.WindSpeed),
                IsFreezing = Imperial.Temperature < 32,
                Latitude = Latitude
            };
            return conditions;
        }

        public CurrentData ToCurrentData()
        {
            return new CurrentData(Pulled, "WUnderground")
            {
                Observed = Epoch.HasValue ? DateTimeOffset.FromUnixTimeSeconds((long)Epoch.Value) : null,
                StationId = StationId,
                WindDirection = new WindDirection(WindDirection),
                Humidity = Humidity,
                CurrentTemperature = new Temperature(Imperial?.Temperature),
                FeelsLike = new Temperature(Imperial?.WindChill),
                HeatIndex = new Temperature(Imperial?.HeatIndex),
                DewPoint = new Temperature(Imperial?.DewPoint),
                UVIndex = Uv,
                Pressure = new Pressure(Imperial?.Pressure, PressureEnum.InchesOfMercury),
                Coordinates = Latitude != null && Longitude != null ? new Coordinates(Latitude.Value, Longitude.Value) : null,
                WeatherConditions = ToWeatherConditions()
            };
        }

        public HistoryLine ToHistoryLine()
        {
            return new HistoryLine(Pulled, "WUnderground")
            {
                Observed = Epoch.HasValue ? DateTimeOffset.FromUnixTimeSeconds((long)Epoch.Value) : null,
                StationId = StationId,
                WindDirection = new WindDirection(WindDirection),
                Humidity = Humidity,
                CurrentTemperature = new Temperature(Imperial?.Temperature),
                FeelsLike = new Temperature(Imperial?.WindChill),
                HeatIndex = new Temperature(Imperial?.HeatIndex),
                DewPoint = new Temperature(Imperial?.DewPoint),
                UVIndex = Uv,
                Pressure = new Pressure(Imperial?.Pressure, PressureEnum.InchesOfMercury),
                Coordinates = Latitude != null && Longitude != null ? new Coordinates(Latitude.Value, Longitude.Value) : null,
                WeatherConditions = ToWeatherConditions()
            };
        }

    }
}
