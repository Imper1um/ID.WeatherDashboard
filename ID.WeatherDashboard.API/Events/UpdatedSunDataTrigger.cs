using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;

namespace ID.WeatherDashboard.API.Events
{
    public class UpdatedSunDataTrigger : DataRetrieverTrigger
    {
        public UpdatedSunDataTrigger(IDataRetrieverService dataRetrieverService,
            Location location) : base(dataRetrieverService, location)
        {
            dataRetrieverService.SunDataUpdated += DataRetrieverService_SunDataUpdated;
        }

        private void DataRetrieverService_SunDataUpdated(object? sender, DataUpdatedEventArgs e)
        {
            if (Location.Equals(e.Location))
#pragma warning disable CS4014
                GetSunData();
#pragma warning restore CS4014
        }

        private async Task GetSunData()
        {
            SunData = await DataRetrieverService.GetSunDataAsync(Location);
            TimeToTrigger = true;
            Unsubscribe();
        }

        public SunData? SunData { get; private set; }

        public override void Unsubscribe()
        {
            DataRetrieverService.SunDataUpdated -= DataRetrieverService_SunDataUpdated;
        }
    }
}
