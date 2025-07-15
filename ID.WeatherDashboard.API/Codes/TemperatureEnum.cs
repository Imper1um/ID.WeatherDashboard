using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Codes
{
    public enum TemperatureEnum
    {
        Fahrenheit,
        Celsius
    }

    public static class TemperatureEnumExtensions
    {
        public static string ToSymbol(this TemperatureEnum temperatureEnum)
        {
            return temperatureEnum switch
            {
                TemperatureEnum.Fahrenheit => "°F",
                TemperatureEnum.Celsius => "°C",
                _ => throw new ArgumentOutOfRangeException(nameof(temperatureEnum), temperatureEnum, null)
            };
        }

        public static string ToFullName(this TemperatureEnum temperatureEnum)
        {
            return temperatureEnum switch
            {
                TemperatureEnum.Fahrenheit => "Fahrenheit",
                TemperatureEnum.Celsius => "Celsius",
                _ => throw new ArgumentOutOfRangeException(nameof(temperatureEnum), temperatureEnum, null)
            };
        }
    }
}
