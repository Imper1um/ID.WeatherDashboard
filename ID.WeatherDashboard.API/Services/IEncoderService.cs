using ID.WeatherDashboard.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Services
{
    public interface IEncoderService
    {
        Task<ImageWeatherData?> GetCurrentImageWeatherDataAsync(string path);
        Task EncodeWeatherDataAsync(string path, ImageWeatherData data);
        Task<ImageWeatherData?> GenerateImageWeatherDataAsync(string path);
    }
}
