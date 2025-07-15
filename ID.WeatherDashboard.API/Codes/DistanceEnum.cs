using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Codes
{
    public enum DistanceEnum
    {
        Miles,
        Kilometers,
        NauticalMiles,
        Meters,
        Feet
    }

    public static class DistanceEnumExtensions
    {
        public static string ToSymbol(this DistanceEnum distanceEnum)
        {
            return distanceEnum switch
            {
                DistanceEnum.Miles => "mi",
                DistanceEnum.Kilometers => "km",
                DistanceEnum.NauticalMiles => "nmi",
                DistanceEnum.Meters => "m",
                DistanceEnum.Feet => "ft",
                _ => throw new ArgumentOutOfRangeException(nameof(distanceEnum), distanceEnum, null)
            };
        }
        public static string ToFullName(this DistanceEnum distanceEnum)
        {
            return distanceEnum switch
            {
                DistanceEnum.Miles => "Miles",
                DistanceEnum.Kilometers => "Kilometers",
                DistanceEnum.NauticalMiles => "Nautical Miles",
                DistanceEnum.Meters => "Meters",
                DistanceEnum.Feet => "Feet",
                _ => throw new ArgumentOutOfRangeException(nameof(distanceEnum), distanceEnum, null)
            };
        }
    }
}
