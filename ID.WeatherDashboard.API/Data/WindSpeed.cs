using ID.WeatherDashboard.API.Codes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    public class WindSpeed
    {
        public WindSpeed(float? speed, WindSpeedEnum storedAs = WindSpeedEnum.MilesPerHour)
        {
            SpeedValue = speed;
            StoredAs = storedAs;
        }

        public float? SpeedValue { get; }
        public WindSpeedEnum StoredAs { get; } = WindSpeedEnum.MilesPerHour;

        public float? To(WindSpeedEnum target)
        {
            return SpeedValue.Convert(StoredAs, target);
        }
    }
}
