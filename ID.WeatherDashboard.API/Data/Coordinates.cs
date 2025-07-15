using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    /// <summary>
    /// Represents a geographic coordinate with latitude and longitude.
    /// </summary>
    public class Coordinates
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Coordinates"/> class with the specified latitude and longitude.
        /// </summary>
        /// <param name="latitude">The latitude component of the coordinates.</param>
        /// <param name="longitude">The longitude component of the coordinates.</param>
        public Coordinates(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary>
        /// Gets or sets the latitude component of the coordinates.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude component of the coordinates.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Returns a string representation of the coordinates in the format "Latitude, Longitude".
        /// </summary>
        /// <returns>A string representation of the coordinates.</returns>
        public override string ToString()
        {
            return $"{Latitude}, {Longitude}";
        }
    }
}
