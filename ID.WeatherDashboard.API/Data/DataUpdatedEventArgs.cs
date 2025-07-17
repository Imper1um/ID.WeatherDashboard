using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    public class DataUpdatedEventArgs : EventArgs
    {
        public DataUpdatedEventArgs(Location location)
        {
            Location = location;
        }

        public Location Location { get; }
    }
}
