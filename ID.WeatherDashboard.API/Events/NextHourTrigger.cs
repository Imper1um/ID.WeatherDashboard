using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Events
{
    public class NextHourTrigger : DelayEventTrigger
    {
        public NextHourTrigger()
        {
            TriggerOn = DateTimeOffset.Now.HourOf().AddHours(1);
        }
    }
}
