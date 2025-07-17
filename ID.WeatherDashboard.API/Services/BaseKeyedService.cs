using ID.WeatherDashboard.API.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Services
{
    public abstract class BaseKeyedService<T> : BaseService<T> where T : KeyedServiceConfig
    {
        public virtual string ApiKey => Config?.ApiKey ?? string.Empty;
    }
}
