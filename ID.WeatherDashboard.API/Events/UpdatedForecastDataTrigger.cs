using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;

namespace ID.WeatherDashboard.API.Events
{
    public class UpdatedForecastDataTrigger : DataRetrieverTrigger
    {
        public UpdatedForecastDataTrigger(IDataRetrieverService dataRetrieverService,
            Location location) : base(dataRetrieverService, location)
        {
            dataRetrieverService.ForecastDataUpdated += DataRetrieverService_ForecastDataUpdated;
        }

        private void DataRetrieverService_ForecastDataUpdated(object? sender, DataUpdatedEventArgs e)
        {
            if (Location.Equals(e.Location))
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                GetForecastData();
#pragma warning restore CS4014
        }

        private async Task GetForecastData()
        {
            ForecastData = await DataRetrieverService.GetForecastDataAsync(Location);
            TimeToTrigger = true;
            Unsubscribe();
        }

        public ForecastData? ForecastData { get; private set; }

        public override void Unsubscribe()
        {
            DataRetrieverService.ForecastDataUpdated -= DataRetrieverService_ForecastDataUpdated;
        }
    }
}