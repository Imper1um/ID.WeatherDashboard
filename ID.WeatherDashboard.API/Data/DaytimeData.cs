using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    /// <summary>
    /// Represents daytime weather data including temperature, precipitation, and forecast description.
    /// </summary>
    public class DaytimeData
    {
        /// <summary>
        /// Gets or sets the high temperature in degrees Fahrenheit.
        /// </summary>
        public Temperature? High { get; set; }

        /// <summary>
        /// Gets or sets the total rainfall in inches.
        /// </summary>
        public Precipitation? Rain { get; set; }

        /// <summary>
        /// Gets or sets the probability of rain as a value between 0 and 1.
        /// </summary>
        [Range(0f, 1f)]
        public float? RainPercentage { get; set; }

        /// <summary>
        /// Gets or sets the total snowfall.
        /// </summary>
        public Precipitation? Snow { get; set; }

        /// <summary>
        /// Gets or sets the probability of snow as a value between 0 and 1.    
        /// </summary>
        [Range(0f, 1f)]
        public float? SnowPercentage { get; set; }

        /// <summary>
        /// Gets or sets the textual description of the forecast.
        /// </summary>
        public string? ForecastText { get; set; }
    }
}
