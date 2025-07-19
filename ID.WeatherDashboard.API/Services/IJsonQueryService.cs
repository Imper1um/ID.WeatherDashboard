using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Services
{
    public interface IJsonQueryService
    {
        Task<Stream?> QueryStreamAsync(string url, params Tuple<string, string>[] parameters);

        Task<string?> QueryStringAsync(string url, params Tuple<string, string>[] headers);

        Task<T?> QueryAsync<T>(string url, params Tuple<string, string>[] headers);
    }
}
