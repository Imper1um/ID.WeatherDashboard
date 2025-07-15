using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Codes
{
    public enum PressureEnum
    {
        Millibars,
        Hectopascals,
        InchesOfMercury,
        Atmospheres
    }

    public static class PressureEnumExtensions
    {
        public static string ToSymbol(this PressureEnum pressureEnum)
        {
            return pressureEnum switch
            {
                PressureEnum.Millibars => "mb",
                PressureEnum.Hectopascals => "hPa",
                PressureEnum.InchesOfMercury => "inHg",
                PressureEnum.Atmospheres => "atm",
                _ => throw new ArgumentOutOfRangeException(nameof(pressureEnum), pressureEnum, null)
            };
        }
        public static string ToFullName(this PressureEnum pressureEnum)
        {
            return pressureEnum switch
            {
                PressureEnum.Millibars => "Millibars",
                PressureEnum.Hectopascals => "Hectopascals",
                PressureEnum.InchesOfMercury => "Inches of Mercury",
                PressureEnum.Atmospheres => "Atmospheres",
                _ => throw new ArgumentOutOfRangeException(nameof(pressureEnum), pressureEnum, null)
            };
        }
    }
}
