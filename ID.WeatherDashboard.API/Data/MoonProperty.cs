using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    public class MoonProperty
    {
        public MoonProperty(double value, DateTimeOffset at)
        {
            Value = value;
            At = at;
        }

        public double Value { get; set; }
        public DateTimeOffset At { get; set; }
    }
}
