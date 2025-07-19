using System.Net;
using System.Text.Json;

namespace ID.WeatherDashboard.API.Services
{
    public class JsonQueryService : IJsonQueryService
    {
        private readonly HttpClient _httpClient;

        public JsonQueryService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> QueryStringAsync(string url, params Tuple<string, string>[] headers)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                AddHeaders(request, headers);

                using var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new QueryFailureException($"Query to {url} failed.", response.StatusCode, content);

                return content;
            }
            catch (Exception ex) when (ex is not QueryFailureException)
            {
                throw new QueryFailureException($"An unexpected error occurred querying {url}.", ex);
            }
        }

        public async Task<Stream> QueryStreamAsync(string url, params Tuple<string, string>[] headers)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                AddHeaders(request, headers);

                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    throw new QueryFailureException($"Query to {url} failed.", response.StatusCode, content);
                }

                return await response.Content.ReadAsStreamAsync();
            }
            catch (Exception ex) when (ex is not QueryFailureException)
            {
                throw new QueryFailureException($"An unexpected error occurred querying {url}.", ex);
            }
        }

        public async Task<T> QueryAsync<T>(string url, params Tuple<string, string>[] headers)
        {
            try
            {
                var jsonString = await QueryStringAsync(url, headers);
                return JsonSerializer.Deserialize<T>(jsonString)
                       ?? throw new QueryFailureException($"Deserialization returned null for {url}.");
            }
            catch (JsonException ex)
            {
                throw new QueryFailureException($"Deserialization failed for {url}.", ex);
            }
        }

        private static void AddHeaders(HttpRequestMessage request, params Tuple<string, string>[] headers)
        {
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Item1, header.Item2);
            }
        }
    }
}
