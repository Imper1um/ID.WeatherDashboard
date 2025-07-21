using ID.WeatherDashboard.API.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Services
{
    public interface IEventControllerService
    {
        Task Start();
        void End();
        void AddEvent(EventAction action);
    }
}
