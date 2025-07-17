using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Codes
{
    public enum AlertCategoryEnum
    {
        /// <summary>
        /// Meterological alerts, such as severe weather warnings.
        /// </summary>
        Met,
        Safety,
        Security,
        Rescue,
        Fire,
        Health,
        /// <summary>
        /// Environmental alerts, such as air quality or pollution warnings.
        /// </summary>
        Env,
        Transport,
        Other,
        Unknown
    }
}
