using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Elements.Background;
using ReactiveUI;
using Avalonia.Controls;
using Location = ID.WeatherDashboard.API.Data.Location;

namespace ID.WeatherDashboard.API.ViewModels
{
    public class DashboardViewModel : ReactiveObject
    {
        public DashboardViewModel()
        {
            if (Design.IsDesignMode)
            {
                Location = new Location("DashboardViewModel") { Latitude = 28.538336, Longitude = -81.379234 };
                BackgroundSelection = new BackgroundSelection()
                {
                    Path = "file:///D:/Projects/ID.WeatherScreen/assets/backgrounds/Cloudy001.jpg",
                    WeatherData = new ImageWeatherData() { Description = "Nothing" }
                };
                Date = DateTime.Now;
                var startHour = DateTimeOffset.Now.HourOf().AddHours(-24);
                
                var random = new Random();

                TemperatureGraph = new TemperatureGraphViewModel(70, 90,
                    [.. Enumerable.Range(1, 24)
                        .Where(hour => hour != 12 && hour != 13 && hour != 3)
                        .Select(hour => new TemperatureViewModel
                        {
                            At = startHour.AddHours(hour - 1),
                            Hour = hour,
                            Temperature = (float)(60 + random.NextDouble() * 35)
                        })]
                )
                {
                    CurrentTemperature = (float)(60 + random.NextDouble() * 35)
                };
                WeatherEmoji = "";
                WeatherIcon = "avares://ID.WeatherDashboard/Assets/Icons/Icon-Sunny.png";

            }
        }

        public Location Location { get; set; } = null!;

        private BackgroundSelection? _backgroundSelection;
        public BackgroundSelection? BackgroundSelection
        {
            get => _backgroundSelection;
            set => this.RaiseAndSetIfChanged(ref _backgroundSelection, value);
        }

        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set
            {
                this.RaiseAndSetIfChanged(ref _date, value);
                DayOfWeek = _date.ToString("dddd");
                DateString = _date.ToString("MMMM dd, yyyy");
                TimeString = _date.ToString("h:mm tt");
            }
        }

        private string _dayOfWeek = DateTime.Now.ToString("dddd");
        public string DayOfWeek
        {
            get => _dayOfWeek;
            set => this.RaiseAndSetIfChanged(ref _dayOfWeek, value);
        }

        private string _dateString = DateTime.Now.ToString("MMMM dd, yyyy");
        public string DateString
        {
            get => _dateString;
            set => this.RaiseAndSetIfChanged(ref _dateString, value);
        }

        private string _timeString = DateTime.Now.ToString("h:mm tt");
        public string TimeString
        {
            get => _timeString;
            set => this.RaiseAndSetIfChanged(ref _timeString, value);
        }

        private TemperatureGraphViewModel _temperatureGraph = new TemperatureGraphViewModel(0, 100);
        public TemperatureGraphViewModel TemperatureGraph
        {
            get => _temperatureGraph;
            set => this.RaiseAndSetIfChanged(ref _temperatureGraph, value);
        }

        private string? _weatherIcon = null;
        public string? WeatherIcon
        {
            get => _weatherIcon;
            set => this.RaiseAndSetIfChanged(ref _weatherIcon, value);
        }

        private string? _weatherEmoji = null;
        public string? WeatherEmoji
        {
            get => _weatherEmoji;
            set => this.RaiseAndSetIfChanged(ref _weatherEmoji, value);
        }
    }
}
