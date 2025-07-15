using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    /// <summary>
    /// Represents nighttime weather data including low temperature and forecast description.
    /// </summary>
    public class NighttimeData
    {
        /// <summary>
        /// Gets or sets the low temperature in degrees Fahrenheit.
        /// </summary>
        public Temperature? Low { get; set; }

        /// <summary>
        /// Gets or sets the textual description of the nighttime forecast.
        /// </summary>
        public string? ForecastText { get; set; }
    }
}
