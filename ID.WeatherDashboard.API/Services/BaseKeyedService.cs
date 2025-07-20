using ID.WeatherDashboard.API.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ID.WeatherDashboard.API.Services
{
    public abstract class BaseKeyedService<T>(ILogger<BaseKeyedService<T>>? logger = null) : BaseService<T>(logger) where T : KeyedServiceConfig
    {
        public virtual string ApiKey => Config?.ApiKey ?? string.Empty;
    }
}
