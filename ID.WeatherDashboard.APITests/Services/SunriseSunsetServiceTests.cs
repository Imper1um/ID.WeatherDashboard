using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.SunriseSunset.Data;
using ID.WeatherDashboard.SunriseSunset.Services;
using GeoTimeZone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ID.WeatherDashboard.APITests.Services
{
    [TestClass]
    public class SunriseSunsetServiceTests
    {
        private Mock<IJsonQueryService> jsonQueryService = null!;
        private SunriseSunsetService service = null!;

        [TestInitialize]
        public void Init()
        {
            jsonQueryService = new Mock<IJsonQueryService>();
            service = new SunriseSunsetService(jsonQueryService.Object);
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldThrowWhenLocationMissingCoordinates()
        {
            var location = new Location("Nowhere");
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.GetSunDataAsync(location, DateTimeOffset.Now, DateTimeOffset.Now));
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldQueryOncePerDay()
        {
            var from = new DateTimeOffset(new DateTime(2024, 1, 1), TimeSpan.Zero);
            var to = from.AddDays(2);
            var result1 = new SunriseSunsetApiResult { Pulled = from, Results = new SunriseSunsetResult { Sunrise = "2024-01-01T06:00:00+00:00", Sunset = "2024-01-01T18:00:00+00:00" } };
            var result2 = new SunriseSunsetApiResult { Pulled = from.AddDays(1), Results = new SunriseSunsetResult { Sunrise = "2024-01-02T06:01:00+00:00", Sunset = "2024-01-02T18:01:00+00:00" } };
            var result3 = new SunriseSunsetApiResult { Pulled = from.AddDays(2), Results = new SunriseSunsetResult { Sunrise = "2024-01-03T06:02:00+00:00", Sunset = "2024-01-03T18:02:00+00:00" } };
            jsonQueryService.SetupSequence(j => j.QueryAsync<SunriseSunsetApiResult>(It.IsAny<string>(), It.IsAny<Tuple<string, string>[]>()))
                .ReturnsAsync(result1)
                .ReturnsAsync(result2)
                .ReturnsAsync(result3);

            var location = new Location("NY") { Latitude = 40.7128, Longitude = -74.0060 };
            var tzid = TimeZoneLookup.GetTimeZone(location.Latitude!.Value, location.Longitude!.Value).Result;
            var expectedUrl = $"{SunriseSunsetService._serviceUrl}?lat={location.Latitude.Value:F6}&lng={location.Longitude.Value:F6}&formatted=0&date={from:yyyy-MM-dd}&timezone={tzid}";

            var data = await service.GetSunDataAsync(location, from, to);

            Assert.IsNotNull(data);
            Assert.AreEqual(3, data.Lines.Count());
            var lines = data.Lines.ToList();
            Assert.AreEqual(result1.Pulled, lines[0].Pulled);
            Assert.AreEqual(result2.Pulled, lines[1].Pulled);
            Assert.AreEqual(result3.Pulled, lines[2].Pulled);
            jsonQueryService.Verify(j => j.QueryAsync<SunriseSunsetApiResult>(expectedUrl, It.IsAny<Tuple<string, string>[]>()), Times.Exactly(3));
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldReturnEmptyDataWhenServiceReturnsNull()
        {
            jsonQueryService.Setup(j => j.QueryAsync<SunriseSunsetApiResult>(It.IsAny<string>(), It.IsAny<Tuple<string, string>[]>()))
                .ReturnsAsync((SunriseSunsetApiResult?)null);

            var location = new Location("NY") { Latitude = 40.7128, Longitude = -74.0060 };
            var from = new DateTimeOffset(new DateTime(2024, 1, 1), TimeSpan.Zero);
            var to = from.AddDays(1);
            var data = await service.GetSunDataAsync(location, from, to);
            Assert.IsNotNull(data);
            Assert.AreEqual(0, data.Lines.Count());
        }
    }
}
