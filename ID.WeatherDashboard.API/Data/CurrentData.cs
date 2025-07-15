using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    public class CurrentData : DataLine
    {
        public CurrentData(DateTimeOffset pulled, params string[] sources) : base(pulled, sources)
        {
        }

    }
}
