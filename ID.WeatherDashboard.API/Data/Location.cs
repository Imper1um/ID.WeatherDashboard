using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    public class Location
    {
        public Location(string name)
        {
            Name = name;
        }
        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public string? Name { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public override string ToString()
        {
            return Name ?? $"{Latitude},{Longitude}";
        }
    }
}
