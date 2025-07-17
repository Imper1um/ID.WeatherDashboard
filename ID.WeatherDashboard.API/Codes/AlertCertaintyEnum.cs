using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Codes
{
    public enum AlertCertaintyEnum
    {
        Observed,
        Likely,
        Possible,
        Unlikely,
        Unknown = 0
    }
}
