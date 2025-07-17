using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Config
{
    public class MoonPhaseConfig : KeyedServiceConfig
    {
        public MoonPhaseConfig()
        {
            Name = "MoonPhase";
            MaxCallsPerDay = 100;
            MaxCallsPerHour = 10;
        }
    }
}
