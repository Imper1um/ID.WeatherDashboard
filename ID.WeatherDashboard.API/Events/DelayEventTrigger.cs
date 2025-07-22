using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Events
{
    public class DelayEventTrigger : EventTrigger
    {
        public DelayEventTrigger() { }
        public DelayEventTrigger(DateTimeOffset triggerOn)
        {
            TriggerOn = triggerOn;
        }
        public DelayEventTrigger(TimeSpan triggerIn)
        {
            TriggerOn = DateTimeOffset.Now.Add(triggerIn);
        }

        public DateTimeOffset TriggerOn { get; set; }

        public override bool IsTimeToTrigger => DateTimeOffset.Now < TriggerOn;
    }
}
