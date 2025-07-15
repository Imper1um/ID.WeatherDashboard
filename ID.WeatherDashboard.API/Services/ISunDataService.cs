using ID.WeatherDashboard.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Services
{
    public interface ISunDataService : IQueryService
    {
        Task<SunData?> GetSunDataAsync(Location location, DateTime from, DateTime to);
    }
}
