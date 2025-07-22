using DynamicData;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace ID.WeatherDashboard.API.ViewModels
{
    public class TemperatureGraphViewModel : ReactiveObject
    {
        public TemperatureGraphViewModel()
        {

        }

        public TemperatureGraphViewModel(float minTemperature, float maxTemperature, params TemperatureViewModel[] temperatures)
        {
            MinTemperature = minTemperature;
            MaxTemperature = maxTemperature;
            Temperatures.AddRange(temperatures);
        }

        private float _minTemperature = 0;
        public float MinTemperature
        {
            get => _minTemperature;
            set => this.RaiseAndSetIfChanged(ref _minTemperature, value);
        }

        private float _maxTemperature = 100;
        public float MaxTemperature
        {
            get => _maxTemperature;
            set => this.RaiseAndSetIfChanged(ref _maxTemperature, value);
        }

        private ObservableCollection<TemperatureViewModel> _temperatures = [];
        public ObservableCollection<TemperatureViewModel> Temperatures
        {
            get => _temperatures;
            set => this.RaiseAndSetIfChanged(ref _temperatures, value);
        }

        private float _currentTemperature = 91;
        public float CurrentTemperature
        {
            get => _currentTemperature;
            set => this.RaiseAndSetIfChanged(ref _currentTemperature, value);
        }
    }
}
