using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;

namespace ID.WeatherDashboard.API.Events
{
    public class UpdatedHistoryDataTrigger : DataRetrieverTrigger
    {
        public UpdatedHistoryDataTrigger(IDataRetrieverService dataRetrieverService,
            Location location) : base(dataRetrieverService, location)
        {
            dataRetrieverService.HistoryDataUpdated += DataRetrieverService_HistoryDataUpdated;
        }

        private void DataRetrieverService_HistoryDataUpdated(object? sender, DataUpdatedEventArgs e)
        {
            if (Location.Equals(e.Location))
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                GetHistoryData();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task GetHistoryData()
        {
            HistoryData = await DataRetrieverService.GetHistoryDataAsync(Location);
            TimeToTrigger = true;
            Unsubscribe();
        }

        public HistoryData? HistoryData { get; private set; }

        public override void Unsubscribe()
        {
            DataRetrieverService.HistoryDataUpdated -= DataRetrieverService_HistoryDataUpdated;
        }
    }
}
