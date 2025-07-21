using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;

namespace ID.WeatherDashboard.API.Events
{
    public class UpdatedAlertDataTrigger : DataRetrieverTrigger
    {
        public UpdatedAlertDataTrigger(IDataRetrieverService dataRetrieverService,
            Location location) : base(dataRetrieverService, location)
        {
            dataRetrieverService.AlertDataUpdated += DataRetrieverService_AlertDataUpdated;
        }

        private void DataRetrieverService_AlertDataUpdated(object? sender, DataUpdatedEventArgs e)
        {
            if (Location.Equals(e.Location))
#pragma warning disable CS4014
                GetAlertData();
#pragma warning restore CS4014
        }

        private async Task GetAlertData()
        {
            AlertData = await DataRetrieverService.GetAlertDataAsync(Location);
            TimeToTrigger = true;
            Unsubscribe();
        }

        public AlertData? AlertData { get; private set; }

        public override void Unsubscribe()
        {
            DataRetrieverService.AlertDataUpdated -= DataRetrieverService_AlertDataUpdated;
        }
    }
}
