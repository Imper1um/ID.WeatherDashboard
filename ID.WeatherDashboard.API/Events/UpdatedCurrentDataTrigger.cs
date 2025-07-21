using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;

namespace ID.WeatherDashboard.API.Events
{
    public class UpdatedCurrentDataTrigger : DataRetrieverTrigger
    {
        public UpdatedCurrentDataTrigger(IDataRetrieverService dataRetrieverService,
            Location location) : base(dataRetrieverService, location)
        {
            dataRetrieverService.CurrentDataUpdated += DataRetrieverService_CurrentDataUpdated;
        }

        private void DataRetrieverService_CurrentDataUpdated(object? sender, DataUpdatedEventArgs e)
        {
            if (Location.Equals(e.Location))
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                GetCurrentData();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task GetCurrentData()
        {
            CurrentData = await DataRetrieverService.GetCurrentDataAsync(Location);
            TimeToTrigger = true;
            Unsubscribe();
        }

        public CurrentData? CurrentData { get; private set; }

        public override void Unsubscribe()
        {
            DataRetrieverService.CurrentDataUpdated -= DataRetrieverService_CurrentDataUpdated;
        }
    }
}
