using ID.WeatherDashboard.API.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ID.WeatherDashboard.API.Services
{
    public abstract class BaseService<T>(ILogger<BaseService<T>>? logger = null) where T : ServiceConfig
    {
        protected ILogger<BaseService<T>> Logger { get; } = logger ?? NullLogger<BaseService<T>>.Instance;

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
                Logger.LogWarning("{Service} is overloaded! Blocked Call.", GetType().Name);
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
