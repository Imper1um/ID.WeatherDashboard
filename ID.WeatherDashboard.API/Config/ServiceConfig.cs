using System.Text.Json.Serialization;

namespace ID.WeatherDashboard.API.Config
{
    public class ServiceConfig
    {
        public required string Assembly { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
        public string? ApiKey { get; set; }
        public string? StationId { get; set; }
        public int MaxCallsPerHour { get; set; }
        public int MaxCallsPerDay { get; set; }
    }
}
