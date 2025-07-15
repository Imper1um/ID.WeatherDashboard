using ID.WeatherDashboard.API.Codes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    /// <summary>
    /// Represents a wind direction in degrees and as a <see cref="WindDirectionEnum"/>.
    /// </summary>
    public class WindDirection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindDirection"/> class with the specified direction in degrees.
        /// </summary>
        /// <param name="direction">The wind direction in degrees from North (0-360).</param>
        public WindDirection(int direction)
        {
            Direction = direction;
        }

        /// <summary>
        /// Gets or sets the wind direction in degrees from North (0-360).
        /// </summary>
        [Range(0f, 360f)]
        public double Direction { get; set; }

        /// <summary>
        /// Gets the wind direction as a <see cref="WindDirectionEnum"/> value.
        /// </summary>
        public WindDirectionEnum DirectionEnum => Direction.ToWindDirection();
    }
}
