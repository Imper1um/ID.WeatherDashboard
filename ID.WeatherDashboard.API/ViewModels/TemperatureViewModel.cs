using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.ViewModels
{
    public class TemperatureViewModel : ReactiveObject
    {
        private DateTimeOffset _at;
        private float? _temperature;
        private int _hour;

        public DateTimeOffset At
        {
            get => _at;
            set => this.RaiseAndSetIfChanged(ref _at, value);
        }

        public float? Temperature
        {
            get => _temperature;
            set => this.RaiseAndSetIfChanged(ref _temperature, value);
        }

        public int Hour
        {
            get => _hour;
            set => this.RaiseAndSetIfChanged(ref _hour, value);
        }
    }
}
