using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Config
{
    public class WUndergroundApiConfig : KeyedServiceConfig
    {
        public WUndergroundApiConfig()
        {
            Name = "WUnderground";
            ApiKey = string.Empty;
            StationId = string.Empty;
            MaxCallsPerDay = 1500;
            MaxCallsPerHour = 100;
        }

        public required string StationId { get; set; }
    }
}
