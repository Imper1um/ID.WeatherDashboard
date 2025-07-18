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
                WeatherConditions = new WeatherConditions(observed ?? DateTimeOffset.Now)
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

        private WeatherConditions GenerateFullyFormedWeatherConditions(
            DateTimeOffset? time = null,
            Precipitation? basePrecipitationRate = null,
            Precipitation? rainRate = null,
            Precipitation? snowRate = null,
            float? cloudCoverPercentage = null,
            MoonPhase? moonPhase = null,
            WindSpeed? windGustSpeed = null,
            WindSpeed? windSpeed = null,
            Distance? visibility = null,
            float? sunAngle = null,
            float? moonAngle = null,
            bool? isLightning = null,
            bool? isFoggy = null,
            bool? isFreezing = null,
            bool? isHail = null,
            bool? isSleet = null,
            bool? isRain = null,
            bool? isWarning = null,
            bool? isHurricane = null,
            bool? isTornado = null,
            string? stateConditions = null,
            double? latitude = null)
        {
            return new WeatherConditions(time ?? DateTimeOffset.Now)
            {
                BasePrecipitationRate = basePrecipitationRate ?? new Precipitation(TestHelpers.RandomFloatBetween(0, 2), PrecipitationEnum.Inches),
                RainRate = rainRate ?? new Precipitation(TestHelpers.RandomFloatBetween(0, 2), PrecipitationEnum.Inches),
                SnowRate = snowRate ?? new Precipitation(TestHelpers.RandomFloatBetween(0, 2), PrecipitationEnum.Inches),
                CloudCoverPercentage = cloudCoverPercentage ?? TestHelpers.RandomFloatBetween(0, 1),
                MoonPhase = moonPhase ?? TestHelpers.RandomEnumValue<MoonPhase>(),
                WindGustSpeed = windGustSpeed ?? new WindSpeed(TestHelpers.RandomFloatBetween(0, 50), WindSpeedEnum.MilesPerHour),
                WindSpeed = windSpeed ?? new WindSpeed(TestHelpers.RandomFloatBetween(0, 30), WindSpeedEnum.MilesPerHour),
                Visibility = visibility ?? new Distance(TestHelpers.RandomFloatBetween(0, 10), DistanceEnum.Miles),
                SunAngle = sunAngle ?? TestHelpers.RandomFloatBetween(-90, 90),
                MoonAngle = moonAngle ?? TestHelpers.RandomFloatBetween(-90, 90),
                IsLightning = isLightning ?? (Random.Shared.Next(0, 2) == 1),
                IsFoggy = isFoggy ?? (Random.Shared.Next(0, 2) == 1),
                IsFreezing = isFreezing ?? (Random.Shared.Next(0, 2) == 1),
                IsHail = isHail ?? (Random.Shared.Next(0, 2) == 1),
                IsSleet = isSleet ?? (Random.Shared.Next(0, 2) == 1),
                IsRain = isRain ?? (Random.Shared.Next(0, 2) == 1),
                IsWarning = isWarning ?? (Random.Shared.Next(0, 2) == 1),
                IsHurricane = isHurricane ?? (Random.Shared.Next(0, 2) == 1),
                IsTornado = isTornado ?? (Random.Shared.Next(0, 2) == 1),
                StateConditions = stateConditions ?? TestHelpers.RandomString(10, TestHelpers.UppercaseLetters),
                Latitude = latitude ?? TestHelpers.RandomDoubleBetween(-90, 90)
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

    }
}
