using ID.WeatherDashboard.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Services
{
    public interface IDataRetrieverService
    {
        Task<CurrentData?> GetCurrentDataAsync(Location location);
        Task<ForecastData?> GetForecastDataAsync(Location location);
        Task<HistoryData?> GetHistoryDataAsync(Location location);
        Task<SunData?> GetSunDataAsync(Location location);
        Task<AlertData?> GetAlertDataAsync(Location location);

        event EventHandler<DataUpdatedEventArgs> CurrentDataUpdated;
        event EventHandler<DataUpdatedEventArgs> ForecastDataUpdated;
        event EventHandler<DataUpdatedEventArgs> HistoryDataUpdated;
        event EventHandler<DataUpdatedEventArgs> SunDataUpdated;
        event EventHandler<DataUpdatedEventArgs> AlertDataUpdated;
    }
}
