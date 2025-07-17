using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    public interface IPulledData
    {
        DateTimeOffset Pulled { get; }
    }
}
