using ID.WeatherDashboard.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Services
{
    public interface IHistoryQueryService : IQueryService
    {
        Task<HistoryData?> GetHistoryDataAsync(Location location, DateTimeOffset from, DateTimeOffset to);
    }
}
