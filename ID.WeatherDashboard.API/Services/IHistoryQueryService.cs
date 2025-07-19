using ID.WeatherDashboard.API.Data;

namespace ID.WeatherDashboard.API.Services
{
    public interface IHistoryQueryService : IQueryService
    {
        Task<HistoryData?> GetHistoryDataAsync(Location location, DateTimeOffset from, DateTimeOffset to);
    }
}
