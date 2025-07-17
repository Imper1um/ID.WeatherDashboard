using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Config
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(WeatherApiConfig))]
    [JsonDerivedType(typeof(SunriseSunsetApiConfig))]
    [JsonDerivedType(typeof(WUndergroundApiConfig))]
    public abstract class ServiceConfig
    {
        public required string Name { get; set; }
        public int MaxCallsPerHour { get; set; }
        public int MaxCallsPerDay { get; set; }
    }
}
