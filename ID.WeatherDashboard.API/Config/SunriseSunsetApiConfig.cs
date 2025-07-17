using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Config
{
    public class SunriseSunsetApiConfig : ServiceConfig
    {
        public SunriseSunsetApiConfig()
        {
            Name = "SunriseSunset";
            MaxCallsPerHour = 10;
            MaxCallsPerDay = 100;
        }
    }
}
