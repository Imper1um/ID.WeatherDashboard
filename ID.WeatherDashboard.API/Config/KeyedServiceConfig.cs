using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Config
{
    public abstract class KeyedServiceConfig : ServiceConfig
    {
        public virtual required string ApiKey { get; set; }
    }
}
