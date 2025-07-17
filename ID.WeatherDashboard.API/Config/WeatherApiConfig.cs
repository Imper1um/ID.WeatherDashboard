using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Config
{
    public class WeatherApiConfig : KeyedServiceConfig
    {
        public WeatherApiConfig()
        {
            Name = "WeatherApi";
            MaxCallsPerDay = 33333;
            MaxCallsPerHour = 1388;
            ApiKey = string.Empty;
        }

    }
}
