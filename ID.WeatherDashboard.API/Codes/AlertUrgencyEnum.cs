using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Codes
{
    public enum AlertUrgencyEnum
    {
        Immediate = 100,
        Expected = 50,
        Future = 25,
        Unknown = 0
    }
}
