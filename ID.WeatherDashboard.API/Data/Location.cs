using ID.WeatherDashboard.API.Codes;
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

            if (Latitude.HasValue && Longitude.HasValue &&
                other.Latitude.HasValue && other.Longitude.HasValue)
            {
                var distance = DistanceTo(other);
                return distance.To(DistanceEnum.Meters) <= 10;
            }

            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as Location);

        public override int GetHashCode()
        {
            if (Latitude.HasValue && Longitude.HasValue)
            {
                double latRounded = Math.Round(Latitude.Value, 4);
                double lonRounded = Math.Round(Longitude.Value, 4);
                return HashCode.Combine(latRounded, lonRounded);
            }
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

        public Distance DistanceTo(Location a)
        {
            if (!a.Latitude.HasValue || !a.Longitude.HasValue ||
                !Latitude.HasValue || !Longitude.HasValue)
                throw new InvalidOperationException("Both locations must have Latitude and Longitude.");

            double R = 6371e3;
            double φ1 = a.Latitude.Value.ToRadians();
            double φ2 = Latitude.Value.ToRadians();
            double Δφ = (Latitude.Value - a.Latitude.Value).ToRadians();
            double Δλ = (Longitude.Value - a.Longitude.Value).ToRadians();

            double h = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                       Math.Cos(φ1) * Math.Cos(φ2) *
                       Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1 - h));
            double distanceMeters = R * c;

            return new Distance((float)distanceMeters, DistanceEnum.Meters);
        }
    }
}
