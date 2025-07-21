using ID.WeatherDashboard.API.Codes;
using ID.WeatherDashboard.API.Data;

namespace ID.WeatherDashboard.API.Services
{
    public class LocationStorageService : ILocationStorageService
    {
        private readonly List<Location> _locations = [];

        public bool UploadGeolocation(Location location)
        {
            if (string.IsNullOrWhiteSpace(location.Name) || location.Latitude == null || location.Longitude == null)
                return false;

            if (_locations.Any(l => l.Name == location.Name))
                return false;

            var stored = new Location(location.Name)
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude
            };

            _locations.Add(stored);
            return true;
        }

        public bool ResolveGeolocation(Location location)
        {
            //Do not resolve unless you have at least Latitude + Longitude OR Name.
            if (string.IsNullOrWhiteSpace(location.Name) && location.Latitude != null && location.Longitude != null)
            {
                return MatchCoordinatesToName(location);
            }
            else if (!string.IsNullOrWhiteSpace(location.Name) && (location.Latitude == null || location.Longitude == null))
            {
                return MatchNameToCoordinates(location);
            }
            return false;
        }

        private bool MatchCoordinatesToName(Location location)
        {
            if (_locations.Any() != true)
                return false;
            var matchingLocation = from locations in _locations
                                   select new
                                   {
                                       Distance = locations.DistanceTo(location),
                                       Location = locations
                                   };

            var closestLocation = matchingLocation.MinBy(x => x.Distance.DistanceValue);
            if (closestLocation!.Distance.To(DistanceEnum.Meters) < 20)
            {
                location.Name = closestLocation.Location.Name;
                return true;
            }
            return false;
        }

        private bool MatchNameToCoordinates(Location location)
        {
            if (_locations.Any() != true)
                return false;
            var matchingLocation = _locations.FirstOrDefault(l => l.Name?.Equals(location.Name, StringComparison.OrdinalIgnoreCase) == true);
            if (matchingLocation != null)
            {
                location.Latitude = matchingLocation.Latitude;
                location.Longitude = matchingLocation.Longitude;
                return true;
            }
            return false;
        }
    }

}
