using ID.WeatherDashboard.API.Codes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    public class Pressure
    {
        public Pressure(float? pressure, PressureEnum storedAs = PressureEnum.Millibars)
        {
            PressureValue = pressure;
            StoredAs = storedAs;
        }
        public float? PressureValue { get;}
        public PressureEnum StoredAs { get; } = PressureEnum.Millibars;
        public float? To(PressureEnum target)
        {
            return PressureValue.Convert(StoredAs, target);
        }
    }
}
