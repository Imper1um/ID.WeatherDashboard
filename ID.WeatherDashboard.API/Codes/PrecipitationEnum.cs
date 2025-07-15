using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Codes
{
    public enum PrecipitationEnum
    {
        Inches,
        Centimeters,
        Millimeters,
        Feet,
        LitersPerSquareMeter
    }

    public static class PrecipitationEnumExtensions
    {
        public static string ToSymbol(this PrecipitationEnum precipitationEnum)
        {
            return precipitationEnum switch
            {
                PrecipitationEnum.Inches => "in",
                PrecipitationEnum.Centimeters => "cm",
                PrecipitationEnum.Millimeters => "mm",
                PrecipitationEnum.Feet => "ft",
                PrecipitationEnum.LitersPerSquareMeter => "L/m²",
                _ => throw new ArgumentOutOfRangeException(nameof(precipitationEnum), precipitationEnum, null)
            };
        }
        public static string ToFullName(this PrecipitationEnum precipitationEnum)
        {
            return precipitationEnum switch
            {
                PrecipitationEnum.Inches => "Inches",
                PrecipitationEnum.Centimeters => "Centimeters",
                PrecipitationEnum.Millimeters => "Millimeters",
                PrecipitationEnum.Feet => "Feet",
                PrecipitationEnum.LitersPerSquareMeter => "Liters per Square Meter",
                _ => throw new ArgumentOutOfRangeException(nameof(precipitationEnum), precipitationEnum, null)
            };
        }
    }
}
