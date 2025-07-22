using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Events
{
    public class NextSecondTrigger : DelayEventTrigger
    {
        public NextSecondTrigger()
        {
            TriggerOn = DateTimeOffset.Now.SecondOf().AddSeconds(1);
        }
    }
}
