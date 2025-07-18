using ID.WeatherDashboard.API.Codes;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using Moq;

namespace ID.WeatherDashboard.APITests.Services
{
    [TestClass]
    public class DataRetrieverServiceTests_History : DataRetrieverServiceTests
    {
        private Mock<IHistoryQueryService> SetupHistoryQueryService(string name)
        {
            var s = new Mock<IHistoryQueryService>();
            s.SetupGet(m => m.ServiceName).Returns(name);
            HistoryQueryServiceMocks.Add(s);
            return s;
        }

        private HistoryLine GenerateHistoryLine(DateTimeOffset? pulled = null, string[]? sources = null, DateTimeOffset? observed = null, Temperature? high = null, Temperature? low = null)
        {
            return new HistoryLine(pulled ?? DateTimeOffset.Now, sources ?? Array.Empty<string>())
            {
                Observed = observed ?? DateTimeOffset.Now,
                High = high,
                Low = low,
                WeatherConditions = GenerateWeatherConditions()
            };
        }

        private HistoryLine GenerateFullyFormedHistoryLine(
            DateTimeOffset? pulled = null,
            string[]? sources = null,
            DateTimeOffset? observed = null,
            Temperature? high = null,
            Temperature? low = null,
            WindDirection? windDirection = null,
            int? humidity = null,
            Temperature? currentTemperature = null,
            Temperature? feelsLike = null,
            Temperature? heatIndex = null,
            Temperature? dewPoint = null,
            float? uvIndex = null,
            Pressure? pressure = null,
            Coordinates? coordinates = null,
            WeatherConditions? weatherConditions = null)
        {
            return new HistoryLine(pulled ?? DateTimeOffset.Now, sources ?? Array.Empty<string>())
            {
                Observed = observed ?? DateTimeOffset.Now,
                High = high ?? new Temperature(TestHelpers.RandomFloatBetween(50, 100)),
                Low = low ?? new Temperature(TestHelpers.RandomFloatBetween(30, 70)),
                WindDirection = windDirection ?? new WindDirection(TestHelpers.RandomFloatBetween(0, 360)),
                Humidity = humidity ?? TestHelpers.RandomIntBetween(20, 100),
                CurrentTemperature = currentTemperature ?? new Temperature(TestHelpers.RandomFloatBetween(30, 100)),
                FeelsLike = feelsLike ?? new Temperature(TestHelpers.RandomFloatBetween(30, 100)),
                HeatIndex = heatIndex ?? new Temperature(TestHelpers.RandomFloatBetween(30, 110)),
                DewPoint = dewPoint ?? new Temperature(TestHelpers.RandomFloatBetween(10, 70)),
                UVIndex = uvIndex ?? TestHelpers.RandomFloatBetween(0, 12),
                Pressure = pressure ?? new Pressure(TestHelpers.RandomFloatBetween(950, 1050), PressureEnum.Millibars),
                Coordinates = coordinates ?? new Coordinates(TestHelpers.RandomDoubleBetween(-90, 90), TestHelpers.RandomDoubleBetween(-180, 180)),
                WeatherConditions = weatherConditions ?? GenerateFullyFormedWeatherConditions()
            };
        }
        

        [TestMethod]
        public async Task GetHistoryDataAsync_ShouldErrorIfNoServicesConfigured()
        {
            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            Config.HistoryData = GenerateAllStarConfig("HistoryTestService");

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => dr.GetHistoryDataAsync(l),
                "Expected InvalidOperationException when no HistoryQueryService is registered.");
        }

        [TestMethod]
        public async Task GetHistoryDataAsync_ShouldErrorIfNoConfigProvided()
        {
            var historyService = SetupHistoryQueryService("HistoryTestService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => dr.GetHistoryDataAsync(l),
                "Expected InvalidOperationException when HistoryData config is missing.");

            historyService.Verify(s => s.GetHistoryDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Never());
        }

        [TestMethod]
        public async Task GetHistoryDataAsync_ShouldErrorIfNoDataReturnedFromServices()
        {
            var historyService = SetupHistoryQueryService("HistoryTestService");

            // Service returns null HistoryData
            historyService.Setup(s => s.GetHistoryDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync((HistoryData?)null);

            Config.HistoryData = GenerateAllStarConfig("HistoryTestService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => dr.GetHistoryDataAsync(l),
                "Expected InvalidOperationException when HistoryQueryService returns no data.");

            historyService.Verify(s => s.GetHistoryDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Once());
        }

        [TestMethod]
        public async Task GetHistoryDataAsync_ShouldReturnHistoryDataWithLinesWhenValidDataReturned()
        {
            var historyService = SetupHistoryQueryService("HistoryTestService");

            var historyLine = GenerateHistoryLine();
            var historyData = new HistoryData(DateTimeOffset.Now, historyLine);

            historyService.Setup(s => s.GetHistoryDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(historyData);

            Config.HistoryData = GenerateAllStarConfig("HistoryTestService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetHistoryDataAsync(l);

            Assert.IsNotNull(result, "Expected HistoryData to be returned.");
            Assert.IsTrue(result.Lines.Any(), "Expected HistoryData to contain at least one HistoryLine.");
            Assert.AreEqual(historyLine.Observed, result.Lines.First().Observed, "Expected HistoryLine Observed time to match.");

            Assert.AreEqual(1, HistoryDataUpdated.Count, "Expected HistoryDataUpdated event to fire once.");
        }

        [TestMethod]
        public async Task GetHistoryDataAsync_ShouldReturnCachedDataIfNotExpired()
        {
            var historyService = SetupHistoryQueryService("HistoryTestService");

            var historyLine = GenerateHistoryLine();
            var initialHistoryData = new HistoryData(DateTimeOffset.Now, historyLine);

            historyService.Setup(s => s.GetHistoryDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(initialHistoryData);

            Config.HistoryData = GenerateAllStarConfig("HistoryTestService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var firstResult = await dr.GetHistoryDataAsync(l);
            Assert.IsNotNull(firstResult);

            var secondResult = await dr.GetHistoryDataAsync(l);
            Assert.IsNotNull(secondResult);

            Assert.AreSame(firstResult, secondResult, "Expected cached HistoryData to be returned.");
            Assert.IsTrue(secondResult.Lines.Any(), "Expected cached HistoryData to still contain lines.");
            historyService.Verify(s => s.GetHistoryDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Once());
            Assert.AreEqual(1, HistoryDataUpdated.Count, "Expected HistoryDataUpdated event to fire once.");
        }

        [TestMethod]
        public async Task GetHistoryDataAsync_ShouldRefreshDataIfExpired()
        {
            var historyService = SetupHistoryQueryService("HistoryTestService");

            var initialLine = GenerateHistoryLine(observed: DateTimeOffset.Now.AddDays(-1));
            var initialHistoryData = new HistoryData(DateTimeOffset.Now, initialLine);

            var refreshedLine = GenerateHistoryLine(observed: DateTimeOffset.Now);
            var refreshedHistoryData = new HistoryData(DateTimeOffset.Now, refreshedLine);

            var historyDataToReturn = initialHistoryData;
            historyService.Setup(s => s.GetHistoryDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(() => historyDataToReturn);

            Config.HistoryData = GenerateAllStarConfig("HistoryTestService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var firstResult = await dr.GetHistoryDataAsync(l);
            Assert.IsNotNull(firstResult);

            firstResult.Pulled = DateTimeOffset.Now.AddHours(-12);

            historyDataToReturn = refreshedHistoryData;

            var secondResult = await dr.GetHistoryDataAsync(l);
            Assert.IsNotNull(secondResult);

            Assert.IsTrue(secondResult.Lines.Any(hl => hl.Observed == refreshedLine.Observed), "Expected refreshed HistoryData with new HistoryLine.");
            historyService.Verify(s => s.GetHistoryDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Exactly(2));
            Assert.AreEqual(2, HistoryDataUpdated.Count, "Expected HistoryDataUpdated event to fire twice.");
        }

        [TestMethod]
        public async Task GetHistoryDataAsync_ShouldReplaceDataIfOverlayIsFalse()
        {
            var historyService = SetupHistoryQueryService("HistoryTestService");

            var initialLine = GenerateHistoryLine(observed: DateTime.Today.AddDays(-1));
            var initialHistoryData = new HistoryData(DateTimeOffset.Now, initialLine);

            var newLine = GenerateHistoryLine(observed: DateTime.Today);
            var refreshedHistoryData = new HistoryData(DateTimeOffset.Now, newLine);

            var historyDataToReturn = initialHistoryData;
            historyService.Setup(s => s.GetHistoryDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(() => historyDataToReturn);

            Config.HistoryData = GenerateAllStarConfig("HistoryTestService");
            Config.HistoryData.OverlayExistingData = false;

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var firstResult = await dr.GetHistoryDataAsync(l);
            Assert.IsNotNull(firstResult);

            firstResult.Pulled = DateTimeOffset.Now.AddHours(-12);

            historyDataToReturn = refreshedHistoryData;

            var secondResult = await dr.GetHistoryDataAsync(l);
            Assert.IsNotNull(secondResult);

            Assert.AreEqual(1, secondResult.Lines.Count(), "Expected exactly one HistoryLine after replace.");
            Assert.AreEqual(newLine.Observed, secondResult.Lines.First().Observed, "Expected replaced HistoryLine to have the refreshed date.");

            historyService.Verify(s => s.GetHistoryDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Exactly(2));
            Assert.AreEqual(2, HistoryDataUpdated.Count, "Expected HistoryDataUpdated event to fire twice.");
        }

        [TestMethod]
        public async Task GetHistoryDataAsync_ShouldMergeDataWithNewOnRefreshIfOverlayIsTrue()
        {
            var historyService = SetupHistoryQueryService("HistoryTestService");

            var initialLine = GenerateHistoryLine(observed: DateTime.Today.AddDays(-1));
            var initialHistoryData = new HistoryData(DateTimeOffset.Now, initialLine);

            var refreshedLine = GenerateHistoryLine(observed: DateTime.Today);
            var refreshedHistoryData = new HistoryData(DateTimeOffset.Now, refreshedLine);

            var historyDataToReturn = initialHistoryData;
            historyService.Setup(s => s.GetHistoryDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(() => historyDataToReturn);

            Config.HistoryData = GenerateAllStarConfig("HistoryTestService");
            Config.HistoryData.OverlayExistingData = true;

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var firstResult = await dr.GetHistoryDataAsync(l);
            Assert.IsNotNull(firstResult);

            firstResult.Pulled = DateTimeOffset.Now.AddHours(-12);

            historyDataToReturn = refreshedHistoryData;

            var secondResult = await dr.GetHistoryDataAsync(l);
            Assert.IsNotNull(secondResult);

            var lines = secondResult.Lines.ToList();

            Assert.AreEqual(2, lines.Count, "Expected both initial and refreshed HistoryLines to be present due to overlay.");
            Assert.IsTrue(lines.Any(sl => sl.Observed == initialLine.Observed), "Initial HistoryLine should remain.");
            Assert.IsTrue(lines.Any(sl => sl.Observed == refreshedLine.Observed), "Refreshed HistoryLine should be added.");

            historyService.Verify(s => s.GetHistoryDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Exactly(2));
            Assert.AreEqual(2, HistoryDataUpdated.Count, "Expected HistoryDataUpdated event to fire twice.");
        }

    }
}
