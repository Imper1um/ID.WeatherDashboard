using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Events
{
    public class NextMinuteTrigger : DelayEventTrigger
    {
        public NextMinuteTrigger()
        {
            TriggerOn = DateTimeOffset.Now.MinuteOf().AddMinutes(1);
        }
    }
}
