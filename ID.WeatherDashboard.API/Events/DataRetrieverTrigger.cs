using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Events
{
    public abstract class DataRetrieverTrigger : EventTrigger
    {
        public DataRetrieverTrigger(IDataRetrieverService dataRetrieverService,
            Location location)
        {
            DataRetrieverService = dataRetrieverService;
            Location = location;
        }

        protected IDataRetrieverService DataRetrieverService { get; }
        protected Location Location { get; }
        public override bool IsTimeToTrigger => TimeToTrigger;
        protected bool TimeToTrigger { get; set; }

        public abstract void Unsubscribe();
    }
}
