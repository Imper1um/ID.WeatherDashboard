using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Events
{
    public abstract class EventTrigger
    {
        public abstract bool IsTimeToTrigger { get; }
    }
}
