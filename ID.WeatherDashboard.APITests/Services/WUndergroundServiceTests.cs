using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.API.Codes;
using ID.WeatherDashboard.WUnderground.Data;
using ID.WeatherDashboard.WUnderground.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ID.WeatherDashboard.APITests.Services
{
    [TestClass]
    public class WUndergroundServiceTests
    {
        private Mock<IJsonQueryService> jsonQuery = null!;
        private WUndergroundService service = null!;

        [TestInitialize]
        public void Init()
        {
            jsonQuery = new Mock<IJsonQueryService>();
            service = new WUndergroundService(jsonQuery.Object);
        }

        private static Observation GenerateObservation()
        {
            var epoch = TestHelpers.RandomDateTimeOffsetBetween(DateTimeOffset.Now.AddHours(-1), DateTimeOffset.Now).ToUnixTimeSeconds();
            var imp = new Imperial
            {
                Temperature = TestHelpers.RandomFloatBetween(-20, 120),
                WindChill = TestHelpers.RandomFloatBetween(-20, 100),
                HeatIndex = TestHelpers.RandomFloatBetween(80, 130),
                DewPoint = TestHelpers.RandomFloatBetween(0, 70),
                WindSpeed = TestHelpers.RandomFloatBetween(0, 40),
                WindGust = TestHelpers.RandomFloatBetween(0, 60),
                Pressure = TestHelpers.RandomFloatBetween(27, 32),
                PrecipitationRate = TestHelpers.RandomFloatBetween(0, 2)
            };
            return new Observation
            {
                Pulled = DateTimeOffset.Now,
                StationId = TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.Digits),
                Epoch = epoch,
                WindDirection = TestHelpers.RandomIntBetween(0, 360),
                Humidity = TestHelpers.RandomFloatBetween(0, 100),
                Latitude = TestHelpers.RandomDoubleBetween(-90, 90),
                Longitude = TestHelpers.RandomDoubleBetween(-180, 180),
                Uv = TestHelpers.RandomFloatBetween(0, 12),
                Imperial = imp
            };
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldRequestCorrectUrlAndMapObservation()
        {
            var stationId = TestHelpers.RandomString(6, TestHelpers.UppercaseLetters, TestHelpers.Digits);
            var apiKey = TestHelpers.RandomString(12, TestHelpers.Digits);
            service.SetServiceConfig(new WUndergroundApiConfig { StationId = stationId, ApiKey = apiKey, Name = "WU" });

            var obs = GenerateObservation();
            var observations = new Observations { Pulled = obs.Pulled, ObservationLines = [ obs ] };
            string? requestedUrl = null;
            jsonQuery.Setup(j => j.QueryAsync<Observations>(It.IsAny<string>(), It.IsAny<Tuple<string,string>[]>() ))
                .Callback<string, Tuple<string,string>[]>((u, _) => requestedUrl = u)
                .ReturnsAsync(observations);

            var result = await service.GetCurrentDataAsync(new Location("Unit"));

            var expectedUrl = $"{WUndergroundService._currentUrl}?stationId={stationId}&format=json&units=e&apiKey={apiKey}&numericPrecision=decimal";
            Assert.AreEqual(expectedUrl, requestedUrl, $"URL mismatch. Expected {expectedUrl} but got {requestedUrl}.");
            Assert.IsNotNull(result, "Service did not return CurrentData as expected.");
            Assert.AreEqual(DateTimeOffset.FromUnixTimeSeconds((long)obs.Epoch!), result!.Observed, "Observed time incorrect.");
            Assert.AreEqual(obs.Pulled, result.Pulled, "Pulled time not preserved.");
            Assert.AreEqual(obs.StationId, result.StationId, "StationId not copied correctly.");
            Assert.AreEqual(obs.WindDirection, result.WindDirection!.Direction, "Wind direction mismatch.");
            Assert.AreEqual(obs.Humidity, result.Humidity, "Humidity mismatch.");
            Assert.AreEqual(obs.Imperial!.Temperature, result.CurrentTemperature!.To(TemperatureEnum.Fahrenheit), "Current temperature mismatch.");
            Assert.AreEqual(obs.Imperial.WindChill, result.FeelsLike!.To(TemperatureEnum.Fahrenheit), "FeelsLike mismatch.");
            Assert.AreEqual(obs.Imperial.HeatIndex, result.HeatIndex!.To(TemperatureEnum.Fahrenheit), "HeatIndex mismatch.");
            Assert.AreEqual(obs.Imperial.DewPoint, result.DewPoint!.To(TemperatureEnum.Fahrenheit), "DewPoint mismatch.");
            Assert.AreEqual(obs.Uv, result.UVIndex, "UV index mismatch.");
            Assert.AreEqual(obs.Imperial.Pressure, result.Pressure!.To(PressureEnum.InchesOfMercury), "Pressure mismatch.");
            Assert.IsNotNull(result.Coordinates, "Coordinates were not created from observation latitude/longitude.");
            Assert.AreEqual(obs.Latitude, result.Coordinates!.Latitude, "Latitude mismatch.");
            Assert.AreEqual(obs.Longitude, result.Coordinates.Longitude, "Longitude mismatch.");
            Assert.IsNotNull(result.WeatherConditions, "WeatherConditions should not be null.");
            var expectedPrecip = new Precipitation(obs.Imperial.PrecipitationRate).To(PrecipitationEnum.Inches);
            var actualPrecip = result.WeatherConditions!.BasePrecipitationRate!.To(PrecipitationEnum.Inches);
            Assert.IsNotNull(actualPrecip, "Precipitation rate missing in result.");
            Assert.AreEqual(expectedPrecip!.Value, actualPrecip!.Value, 0.0001f, "Precipitation rate mismatch.");
            Assert.AreEqual(obs.Imperial.WindGust, result.WeatherConditions.WindGustSpeed!.To(WindSpeedEnum.MilesPerHour), "Wind gust mismatch.");
            Assert.AreEqual(obs.Imperial.WindSpeed, result.WeatherConditions.WindSpeed!.To(WindSpeedEnum.MilesPerHour), "Wind speed mismatch.");
            Assert.AreEqual(obs.Imperial.Temperature < 32, result.WeatherConditions.IsFreezing, "Freezing flag mismatch.");
            Assert.AreEqual(obs.Latitude, result.WeatherConditions.Latitude, "Condition latitude mismatch.");
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldReturnNullWhenQueryReturnsNull()
        {
            service.SetServiceConfig(new WUndergroundApiConfig { StationId = "id", ApiKey = "key", Name = "WU" });
            jsonQuery.Setup(j => j.QueryAsync<Observations>(It.IsAny<string>(), It.IsAny<Tuple<string,string>[]>() ))
                .ReturnsAsync((Observations?)null);
            var result = await service.GetCurrentDataAsync(new Location("Unit"));
            Assert.IsNull(result, "Expected null CurrentData when query service returns null.");
        }

        [TestMethod]
        public async Task GetHistoryDataAsync_ShouldRequestCorrectUrlAndMapObservations()
        {
            var stationId = TestHelpers.RandomString(6, TestHelpers.UppercaseLetters, TestHelpers.Digits);
            var apiKey = TestHelpers.RandomString(10, TestHelpers.Digits);
            service.SetServiceConfig(new WUndergroundApiConfig { StationId = stationId, ApiKey = apiKey, Name = "WU" });

            var obs1 = GenerateObservation();
            var obs2 = GenerateObservation();
            var observations = new Observations { Pulled = DateTimeOffset.Now, ObservationLines = [ obs1, obs2 ] };
            string? url = null;
            jsonQuery.Setup(j => j.QueryAsync<Observations>(It.IsAny<string>(), It.IsAny<Tuple<string,string>[]>() ))
                .Callback<string, Tuple<string,string>[]>((u, _) => url = u)
                .ReturnsAsync(observations);

            var from = DateTimeOffset.Now.Date.AddDays(-1);
            var to = DateTimeOffset.Now.Date;
            var result = await service.GetHistoryDataAsync(new Location("Hist"), from, to);

            var expectedUrl = $"{WUndergroundService._historyUrl}?stationId={stationId}&format=json&units=e&apiKey={apiKey}&numericPrecision=decimal&startDate={from:yyyyMMdd}&endDate={to:yyyyMMdd}";
            Assert.AreEqual(expectedUrl, url, $"URL mismatch. Expected {expectedUrl} but got {url}.");
            Assert.IsNotNull(result, "Expected HistoryData to be returned.");
            var lines = result!.Lines.ToArray();
            Assert.AreEqual(2, lines.Length, $"Expected two history lines, got {lines.Length}.");
            Assert.AreEqual(DateTimeOffset.FromUnixTimeSeconds((long)obs1.Epoch!), lines[0].Observed, "First history line observed mismatch.");
            Assert.AreEqual(obs1.Imperial!.Temperature, lines[0].CurrentTemperature!.To(TemperatureEnum.Fahrenheit), "First history line temperature mismatch.");
            Assert.AreEqual(DateTimeOffset.FromUnixTimeSeconds((long)obs2.Epoch!), lines[1].Observed, "Second history line observed mismatch.");
            Assert.AreEqual(obs2.Imperial!.Temperature, lines[1].CurrentTemperature!.To(TemperatureEnum.Fahrenheit), "Second history line temperature mismatch.");
            CollectionAssert.AreEqual(new[] { "WUnderground" }, result.Sources.ToArray(), "Sources should be WUnderground only.");
        }
    }
}
