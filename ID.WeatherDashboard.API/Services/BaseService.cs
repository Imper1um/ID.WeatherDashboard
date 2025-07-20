using ID.WeatherDashboard.API.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Services
{
    public abstract class BaseService<T> where T : ServiceConfig
    {
        protected abstract string BaseServiceName { get; }

        public T? Config { get; set; }

        public virtual string ServiceName => Config?.Name ?? BaseServiceName;
        

        private List<DateTimeOffset> Calls { get; } = new List<DateTimeOffset>();

        public bool IsOverloaded => Calls.Count(c => c > DateTimeOffset.Now.AddMinutes(-60)) >= (Config?.MaxCallsPerHour ?? 0)
            && Calls.Count(c => c > DateTimeOffset.Now.AddHours(-24)) >= (Config?.MaxCallsPerDay ?? 0);

        protected bool TryCall()
        {
            if (IsOverloaded)
            {
                //TODO: Install serilog and convert this to Serilog.
                Console.WriteLine($"{GetType().Name} is overloaded! Blocked Call.");
                return false;
            }
            Calls.Add(DateTimeOffset.Now);
            Calls.RemoveAll(c => c < DateTimeOffset.Now.AddHours(-24));
            return true;
        }

        public void SetServiceConfig(ServiceConfig config)
        {
            if (config is T c)
            {
                Config = c;
            }
        }
    }
}
