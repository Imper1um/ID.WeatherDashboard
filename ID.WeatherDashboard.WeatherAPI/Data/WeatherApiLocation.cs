using ID.WeatherDashboard.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.WeatherAPI.Data
{
    public class WeatherApiLocation
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("region")]
        public string? Region { get; set; }
        [JsonPropertyName("country")]
        public string? Country { get; set; }
        [JsonPropertyName("lat")]
        public double? Latitude { get; set; }
        [JsonPropertyName("lon")]
        public double? Longitude { get; set; }
        [JsonPropertyName("tz_id")]
        public string? TimezoneId { get; set; }
        [JsonPropertyName("localtime_epoch")]
        public double? LocalTimeEpoch { get; set; }
        [JsonPropertyName("localtime")]
        public string? LocalTime { get; set; }

        public Coordinates ToCoordinates()
        {
            if (Latitude.HasValue && Longitude.HasValue)
            {
                return new Coordinates(Latitude.Value, Longitude.Value);
            }
            throw new InvalidOperationException("Latitude and Longitude must be set to convert to Coordinates.");
        }
}
