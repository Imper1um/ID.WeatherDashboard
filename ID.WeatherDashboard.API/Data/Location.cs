using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    public class Location : IEquatable<Location>, IComparable<Location>
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
            return (Latitude == null || Longitude == null) ? (Name ?? string.Empty) : $"{Latitude},{Longitude}";
        }

        public bool Equals(Location? other)
        {
            if (other is null) return false;
            if (Latitude.HasValue && Longitude.HasValue && other.Latitude.HasValue && other.Longitude.HasValue)
                return Latitude.Value.Equals(other.Latitude.Value) && Longitude.Value.Equals(other.Longitude.Value);
            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as Location);

        public override int GetHashCode()
        {
            if (Latitude.HasValue && Longitude.HasValue)
                return HashCode.Combine(Latitude.Value, Longitude.Value);
            return Name?.ToLowerInvariant().GetHashCode() ?? 0;
        }

        public int CompareTo(Location? other)
        {
            if (other == null) return 1;
            if (Latitude.HasValue && Longitude.HasValue && other.Latitude.HasValue && other.Longitude.HasValue)
            {
                int latComp = Latitude.Value.CompareTo(other.Latitude.Value);
                if (latComp != 0) return latComp;
                return Longitude.Value.CompareTo(other.Longitude.Value);
            }
            return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
