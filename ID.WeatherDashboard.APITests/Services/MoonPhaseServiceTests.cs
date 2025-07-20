using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.MoonPhase.Data;
using ID.WeatherDashboard.MoonPhase.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace ID.WeatherDashboard.APITests.Services
{
    [TestClass]
    public class MoonPhaseServiceTests
    {
        private class QueueMessageHandler : HttpMessageHandler
        {
            private readonly Queue<HttpResponseMessage> _responses;
            public List<HttpRequestMessage> Requests { get; } = new();
            public QueueMessageHandler(IEnumerable<HttpResponseMessage> responses)
            {
                _responses = new Queue<HttpResponseMessage>(responses);
            }
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add(request);
                if (_responses.Count == 0)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
                }
                return Task.FromResult(_responses.Dequeue());
            }
        }

        private static MoonPhaseAdvancedAPI GenerateApi(DateTime date)
        {
            var sunrise = TestHelpers.RandomDateTimeOffsetBetween(new(date, TimeSpan.Zero), new(date.AddHours(12), TimeSpan.Zero));
            var sunset = TestHelpers.RandomDateTimeOffsetBetween(sunrise, new(date.AddHours(23), TimeSpan.Zero));
            var solarNoon = sunrise + (sunset - sunrise) / 2;
            var moonrise = TestHelpers.RandomDateTimeOffsetBetween(new(date, TimeSpan.Zero), new(date.AddHours(12), TimeSpan.Zero));
            var moonset = TestHelpers.RandomDateTimeOffsetBetween(moonrise, new(date.AddDays(1), TimeSpan.Zero));
            var lat = TestHelpers.RandomDoubleBetween(-90, 90);
            var lon = TestHelpers.RandomDoubleBetween(-180, 180);

            return new MoonPhaseAdvancedAPI(date)
            {
                Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                Location = new MoonPhaseLocation { Latitude = lat, Longitude = lon },
                Sun = new MoonPhaseSun
                {
                    Sunrise = sunrise.ToUnixTimeSeconds(),
                    Sunset = sunset.ToUnixTimeSeconds(),
                    SolarNoon = solarNoon.ToString("HH:mm")
                },
                Moon = new MoonPhaseMoon
                {
                    MoonriseTimestamp = moonrise.ToUnixTimeSeconds(),
                    MoonsetTimestamp = moonset.ToUnixTimeSeconds(),
                    PhaseName = "Full Moon",
                    MoonAzimuth = TestHelpers.RandomDoubleBetween(0, 360),
                    MoonParallacticAngle = TestHelpers.RandomDoubleBetween(-180, 180),
                    MoonDistance = TestHelpers.RandomDoubleBetween(100000, 500000),
                    MoonAltitude = TestHelpers.RandomDoubleBetween(-90, 90)
                }
            };
        }

        private static string SerializeForApi(MoonPhaseAdvancedAPI api)
        {
            var payload = new
            {
                forDateTime = api.For,
                pulled = api.Pulled,
                sun = api.Sun,
                moon = api.Moon,
                moon_phases = api.MoonPhases,
                location = api.Location,
                timestamp = api.Timestamp,
                datestamp = api.Datestamp
            };
            return JsonSerializer.Serialize(payload);
        }

        [TestMethod]
        public async Task GetSunDataAsync_ReturnsNull_WhenLocationInvalid()
        {
            var handler = new QueueMessageHandler(Array.Empty<HttpResponseMessage>());
            var service = new MoonPhaseService(new HttpClient(handler))
            {
                Config = new MoonPhaseConfig { ApiKey = "test", Name = "MoonPhase", MaxCallsPerHour = 0, MaxCallsPerDay = 0 }
            };

            var result = await service.GetSunDataAsync(new Location("Test"), DateTimeOffset.Now, DateTimeOffset.Now);
            Assert.IsNull(result, "Expected null when location lacks coordinates.");
            Assert.AreEqual(0, handler.Requests.Count, "No HTTP requests should be made when location is invalid.");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ParsesApiDataIntoSunData()
        {
            var date = DateTime.Today;
            var api = GenerateApi(date);
            var json = SerializeForApi(api);
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(json) };
            var handler = new QueueMessageHandler(new[] { response });
            var client = new HttpClient(handler);
            var service = new MoonPhaseService(client)
            {
                AdvancedUrl = "http://localhost",
                Config = new MoonPhaseConfig { ApiKey = "key", Name = "MoonPhase", MaxCallsPerHour = 0, MaxCallsPerDay = 0 }
            };

            var location = new Location(api.Location!.Latitude!.Value, api.Location.Longitude!.Value);
            var result = await service.GetSunDataAsync(location, date, date);

            var expected = api.ToSunData(date)!;
            Assert.IsNotNull(result, "Service should return SunData for valid input.");
            Assert.AreEqual(1, result.Lines.Count(), "Exactly one SunLine should be returned.");
            var line = result.Lines.First();
            var expectedLine = expected.Lines.First();
            Assert.AreEqual(expectedLine.Sunrise, line.Sunrise, $"Sunrise mismatch: expected {expectedLine.Sunrise}, got {line.Sunrise}.");
            Assert.AreEqual(expectedLine.Sunset, line.Sunset, $"Sunset mismatch: expected {expectedLine.Sunset}, got {line.Sunset}.");
            Assert.AreEqual(expectedLine.SolarNoon, line.SolarNoon, $"SolarNoon mismatch: expected {expectedLine.SolarNoon}, got {line.SolarNoon}.");
            Assert.IsNotNull(line.MoonData, "MoonData should not be null.");
            Assert.AreEqual(expectedLine.MoonData!.MoonPhase, line.MoonData!.MoonPhase, "MoonPhase conversion incorrect.");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ReturnsLinesForEachDay()
        {
            var start = DateTime.Today;
            var api1 = GenerateApi(start);
            var api2 = GenerateApi(start.AddDays(1));
            var responses = new[]
            {
                new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(SerializeForApi(api1)) },
                new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(SerializeForApi(api2)) }
            };
            var handler = new QueueMessageHandler(responses);
            var service = new MoonPhaseService(new HttpClient(handler))
            {
                AdvancedUrl = "http://localhost",
                Config = new MoonPhaseConfig { ApiKey = "key", Name = "MoonPhase", MaxCallsPerHour = 0, MaxCallsPerDay = 0 }
            };
            var location = new Location(api1.Location!.Latitude!.Value, api1.Location.Longitude!.Value);
            var result = await service.GetSunDataAsync(location, start, start.AddDays(1));

            Assert.IsNotNull(result, "Expected SunData to be returned for range request.");
            Assert.AreEqual(2, result.Lines.Count(), $"Expected two SunLines but found {result.Lines.Count()}.");
            Assert.IsTrue(result.Lines.Any(l => l.For == start), "First day data missing.");
            Assert.IsTrue(result.Lines.Any(l => l.For == start.AddDays(1)), "Second day data missing.");
        }
    }
}
