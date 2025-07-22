using ID.WeatherDashboard.API.Events;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.API.ViewModels;

namespace ID.WeatherDashboard.API.Elements
{
    public class TimeElement(IEventControllerService eventControllerService) : IElementService
    {
        private readonly IEventControllerService EventControllerService = eventControllerService;

        private DashboardViewModel ViewModel { get; set; } = null!;

        public Task StartAsync(DashboardViewModel viewModel)
        {
            UpdateCurrentTime();
            return Task.CompletedTask;
        }

        private void UpdateCurrentTime()
        {
            ViewModel.Date = DateTime.Now;
            EventControllerService.AddEvent(new EventAction(OnTrigger, new NextSecondTrigger()));
        }

        private void OnTrigger(EventAction action, EventTrigger trigger)
        {
            UpdateCurrentTime();
        }
    }
}
