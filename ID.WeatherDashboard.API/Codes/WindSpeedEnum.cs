using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Codes
{
    public enum WindSpeedEnum
    {
        MilesPerHour,
        KilometersPerHour,
        MetersPerHour,
        MetersPerSecond
    }

    public static class WindSpeedEnumExtensions
    {
        public static string ToSymbol(this WindSpeedEnum windEnum)
        {
            return windEnum switch
            {
                WindSpeedEnum.MilesPerHour => "mph",
                WindSpeedEnum.KilometersPerHour => "kph",
                WindSpeedEnum.MetersPerHour => "m/s",
                _ => throw new ArgumentOutOfRangeException(nameof(windEnum), windEnum, null)
            };
        }
        public static string ToFullName(this WindSpeedEnum windEnum)
        {
            return windEnum switch
            {
                WindSpeedEnum.MilesPerHour => "Miles per Hour",
                WindSpeedEnum.KilometersPerHour => "Kilometers per Hour",
                WindSpeedEnum.MetersPerHour => "Meters per Hour",
                _ => throw new ArgumentOutOfRangeException(nameof(windEnum), windEnum, null)
            };
        }
    }
}
