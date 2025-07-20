using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using Moq;
using System.Reflection;
using System.Text.Json;
using MoonPhaseEnum = ID.WeatherDashboard.API.Data.MoonPhase;

namespace ID.WeatherDashboard.APITests.Services
{
    [TestClass]
    public class DataRetrieverServiceTests_Sun : DataRetrieverServiceTests
    {
        private DateTimeOffset RandomTimeBetween(DateTimeOffset start, DateTimeOffset end)
        {
            var totalSeconds = (end - start).TotalSeconds;
            var randomSeconds = Random.Shared.NextDouble() * totalSeconds;
            return start.AddSeconds(randomSeconds);
        }

        private MoonData GenerateFullyFormedMoonData(
            DateTimeOffset? pulled = null,
            string[]? sources = null,
            DateTime? forDatetime = null,
            double? latitude = null,
            double? longitude = null,
            DateTimeOffset? moonrise = null,
            DateTimeOffset? moonset = null,
            MoonPhaseEnum? moonPhase = null,
            MoonProperty? moonDeclination = null,
            MoonProperty? moonAzimuth = null,
            MoonProperty? moonParallacticAngle = null,
            MoonProperty? moonDistance = null,
            MoonProperty? moonAltitude = null)
        {
            var now = DateTimeOffset.Now;
            var start = new DateTimeOffset(forDatetime ?? DateTime.Today, now.Offset);
            var end = start.AddHours(24);

            var lat = latitude ?? (Random.Shared.NextDouble() * 180) - 90;
            var lon = longitude ?? (Random.Shared.NextDouble() * 360) - 180;
            var rise = moonrise ?? RandomTimeBetween(start, end.AddHours(-20));
            var set = moonset ?? RandomTimeBetween(rise, end);

            var phase = moonPhase ?? TestHelpers.RandomEnumValue<MoonPhaseEnum>();

            var declination = moonDeclination ?? new MoonProperty(Random.Shared.NextDouble() * 360 - 180, RandomTimeBetween(start, end));
            var azimuth = moonAzimuth ?? new MoonProperty(Random.Shared.NextDouble() * 360, RandomTimeBetween(start, end));
            var parallactic = moonParallacticAngle ?? new MoonProperty(Random.Shared.NextDouble() * 360, RandomTimeBetween(start, end));
            var distance = moonDistance ?? new MoonProperty(Random.Shared.NextDouble() * 500000 + 100000, RandomTimeBetween(start, end)); // 100,000 km to 600,000 km
            var altitude = moonAltitude ?? new MoonProperty(Random.Shared.NextDouble() * 180 - 90, RandomTimeBetween(start, end));

            return GenerateMoonData(
                pulled,
                sources,
                forDatetime,
                lat,
                lon,
                rise,
                set,
                phase,
                declination,
                azimuth,
                parallactic,
                distance,
                altitude
            );
        }


        private MoonData GenerateMoonData(DateTimeOffset? pulled = null, string[]? sources = null, DateTime? forDatetime = null, double? latitude = null, double? longitude = null, DateTimeOffset? moonrise = null, DateTimeOffset? moonset = null, MoonPhaseEnum? moonPhase = null,
            MoonProperty? moonDeclination = null, MoonProperty? moonAzimuth = null, MoonProperty? moonParallacticAngle = null, MoonProperty? moonDistance = null, MoonProperty? moonAltitude = null)
        {
            return new MoonData(pulled ?? DateTimeOffset.Now, sources ?? Array.Empty<string>())
            {
                For = forDatetime ?? DateTime.Today,
                Latitude = latitude,
                Longitude = longitude,
                Moonrise = moonrise,
                Moonset = moonset,
                MoonPhase = moonPhase,
                MoonDeclination = moonDeclination,
                MoonAzimuth = moonAzimuth,
                MoonParallacticAngle = moonParallacticAngle,
                MoonDistance = moonDistance,
                MoonAltitude = moonAltitude
            };
        }

        private SunLine GenerateFullyFormedSunLine(DateTimeOffset? pulled = null, string[]? sources = null,
            DateTimeOffset? astronomicalTwilightBegin = null,
            DateTimeOffset? nauticalTwilightBegin = null,
            DateTimeOffset? civilTwilightBegin = null,
            DateTimeOffset? sunrise = null,
            DateTimeOffset? dayStart = null,
            DateTimeOffset? solarNoon = null,
            DateTimeOffset? dayEnd = null,
            DateTimeOffset? sunset = null,
            DateTimeOffset? civilTwilightEnd = null,
            DateTimeOffset? nauticalTwilightEnd = null,
            DateTimeOffset? astronomicalTwilightEnd = null,
            MoonData? moonData = null,
            double? latitude = null,
            double? sunAzimuth = null,
            DateTime? forDatetime = null)
        {
            var now = DateTimeOffset.Now;
            var start = new DateTimeOffset(forDatetime ?? DateTime.Today, now.Offset);
            var end = start.AddHours(24);
            var astroBeginR = astronomicalTwilightBegin ?? RandomTimeBetween(start, end.AddHours(-23));
            var nauticalBeginR = nauticalTwilightBegin ?? RandomTimeBetween(astroBeginR, end.AddHours(-22));
            var civilBeginR = civilTwilightBegin ?? RandomTimeBetween(nauticalBeginR, end.AddHours(-21));
            var sunriseR = sunrise ?? RandomTimeBetween(civilBeginR, end.AddHours(-20));
            var dayStartR = dayStart ?? RandomTimeBetween(sunriseR, end.AddHours(-19));
            var solarNoonR = solarNoon ?? RandomTimeBetween(dayStartR, end.AddHours(-12));
            var dayEndR = dayEnd ?? RandomTimeBetween(solarNoonR, end.AddHours(-5));
            var sunsetR = sunset ?? RandomTimeBetween(dayEndR, end.AddHours(-4));
            var civilEndR = civilTwilightEnd ?? RandomTimeBetween(sunsetR, end.AddHours(-3));
            var nauticalEndR = nauticalTwilightEnd ?? RandomTimeBetween(civilEndR, end.AddHours(-2));
            var astroEndR = astronomicalTwilightEnd ?? RandomTimeBetween(nauticalEndR, end);
            return GenerateSunLine(pulled, sources,
                astroBeginR, nauticalBeginR, civilBeginR,
                sunriseR, dayStartR, solarNoonR, dayEndR,
                sunsetR, civilTwilightEnd, nauticalEndR, astroEndR,
                moonData ?? GenerateFullyFormedMoonData(pulled, sources, latitude: latitude),
                latitude = latitude ?? (Random.Shared.NextDouble() * 180) - 90,
                sunAzimuth = sunAzimuth ?? (Random.Shared.NextDouble() * 360),
                forDatetime = forDatetime ?? DateTime.Now);
        }

        private SunLine GenerateSunLine(DateTimeOffset? pulled = null, string[]? sources = null, 
            DateTimeOffset? astronomicalTwilightBegin = null,
            DateTimeOffset? nauticalTwilightBegin = null,
            DateTimeOffset? civilTwilightBegin = null,
            DateTimeOffset? sunrise = null,
            DateTimeOffset? dayStart = null,
            DateTimeOffset? solarNoon = null,
            DateTimeOffset? dayEnd = null,
            DateTimeOffset? sunset = null,
            DateTimeOffset? civilTwilightEnd = null,
            DateTimeOffset? nauticalTwilightEnd = null,
            DateTimeOffset? astronomicalTwilightEnd = null,
            MoonData? moonData = null,
            double? latitude = null,
            double? sunAzimuth = null,
            DateTime? forDatetime = null)
        {
            return new SunLine(pulled ?? DateTimeOffset.Now, sources ?? Array.Empty<string>())
            {
                AstronomicalTwilightBegin = astronomicalTwilightBegin,
                NauticalTwilightBegin = nauticalTwilightBegin,
                CivilTwilightBegin = civilTwilightBegin,
                Sunrise = sunrise,
                DayStart = dayStart,
                SolarNoon = solarNoon,
                DayEnd = dayEnd,
                Sunset = sunset,
                CivilTwilightEnd = civilTwilightEnd,
                NauticalTwilightEnd = nauticalTwilightEnd,
                AstronomicalTwilightEnd = astronomicalTwilightEnd,
                MoonData = moonData ?? GenerateMoonData(pulled, sources, latitude: latitude),
                Latitude = latitude,
                SunAzimuth = sunAzimuth,
                For = forDatetime ?? DateTime.Today
            };
        }

        private Mock<ISunDataService> SetupSunDataService(string name)
        {
            var s = new Mock<ISunDataService>();
            s.SetupGet(m => m.ServiceName).Returns(name);
            SunDataServiceMocks.Add(s);
            return s;
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldErrorIfNoServices()
        {
            var dr = GetDataRetriever();
            var l = new Location("TestLocation");
            Config.SunData = GenerateAllStarConfig("SunTestService");

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => dr.GetSunDataAsync(l),
                "Expected InvalidOperationException when no SunData services are registered.");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldErrorIfNoConfig()
        {
            var sunService = SetupSunDataService("SunTestService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => dr.GetSunDataAsync(l),
                "Expected InvalidOperationException when SunData config is missing.");

            sunService.Verify(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Never());
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldErrorIfNoDataReturnedFromServices()
        {
            var sunService = SetupSunDataService("SunTestService");

            sunService.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync((SunData?)null);

            Config.SunData = GenerateAllStarConfig("SunTestService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => dr.GetSunDataAsync(l),
                "Expected InvalidOperationException when SunData services return no data.");

            sunService.Verify(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Once());
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldReturnSunDataWithLinesWhenValidDataReturned()
        {
            var sunService = SetupSunDataService("SunTestService");

            var sunLine = GenerateSunLine();
            var sunData = new SunData(DateTimeOffset.Now, sunLine);

            sunService.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(sunData);

            Config.SunData = GenerateAllStarConfig("SunTestService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetSunDataAsync(l);

            Assert.IsNotNull(result, "Expected SunData to be returned.");
            Assert.IsTrue(result.Lines.Any(), "Expected SunData to contain at least one SunLine.");
            Assert.AreEqual(sunLine.For, result.Lines.First().For, "Expected SunLine 'For' date to match.");

            Assert.AreEqual(1, SunDataUpdated.Count, "Expected SunDataUpdated event to fire once.");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldReturnEmptySunDataWhenAllDataLinesAreEmpty()
        {
            var sunService = SetupSunDataService("SunTestService");

            var emptySunData = new SunData(DateTimeOffset.Now);

            sunService.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(emptySunData);

            Config.SunData = GenerateAllStarConfig("SunTestService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetSunDataAsync(l);

            Assert.IsNotNull(result, "Expected SunData to be returned even if no lines are present.");
            Assert.IsFalse(result.Lines.Any(), "Expected no SunLines in SunData.");
            Assert.AreEqual(1, SunDataUpdated.Count, "Expected SunDataUpdated event to fire once.");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldReturnCachedDataIfNotExpired()
        {
            var sunService = SetupSunDataService("SunTestService");

            var sunLine = GenerateSunLine();
            var initialSunData = new SunData(DateTimeOffset.Now, sunLine);

            sunService.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(initialSunData);

            Config.SunData = GenerateAllStarConfig("SunTestService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var firstResult = await dr.GetSunDataAsync(l);
            Assert.IsNotNull(firstResult);

            var firstPulled = firstResult.Pulled;

            var secondResult = await dr.GetSunDataAsync(l);
            Assert.IsNotNull(secondResult);

            Assert.AreSame(firstResult, secondResult, "Expected cached SunData to be returned.");
            sunService.Verify(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Once());

            Assert.AreEqual(1, SunDataUpdated.Count, "Expected SunDataUpdated event to fire once.");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldRefreshDataIfExpired()
        {
            var sunService = SetupSunDataService("SunTestService");

            var initialSunLine = GenerateSunLine(forDatetime: DateTime.Today);
            var initialSunData = new SunData(DateTimeOffset.Now, initialSunLine);

            var refreshedSunLine = GenerateSunLine(forDatetime: DateTime.Today.AddDays(1));
            var refreshedSunData = new SunData(DateTimeOffset.Now, refreshedSunLine);

            var sunDataToReturn = initialSunData;
            sunService.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(() => sunDataToReturn);

            Config.SunData = GenerateAllStarConfig("SunTestService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var firstResult = await dr.GetSunDataAsync(l);
            Assert.IsNotNull(firstResult);

            firstResult.Pulled = DateTimeOffset.Now.AddHours(-12);

            sunDataToReturn = refreshedSunData;

            var secondResult = await dr.GetSunDataAsync(l);
            Assert.IsNotNull(secondResult);

            Assert.IsTrue(secondResult.Lines.Any(l => l.For == DateTime.Today.AddDays(1)), "Expected refreshed SunData with new SunLine.");
            sunService.Verify(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Exactly(2));

            Assert.AreEqual(2, SunDataUpdated.Count, "Expected SunDataUpdated event to fire twice.");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldReplaceDataIfOverlayIsFalse()
        {
            var sunService = SetupSunDataService("SunTestService");

            var initialSunLine = GenerateSunLine(forDatetime: DateTime.Today);
            var initialSunData = new SunData(DateTimeOffset.Now, initialSunLine);

            var newSunLine = GenerateSunLine(forDatetime: DateTime.Today.AddDays(1));
            var refreshedSunData = new SunData(DateTimeOffset.Now, newSunLine);

            var sunDataToReturn = initialSunData;
            sunService.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(() => sunDataToReturn);

            Config.SunData = GenerateAllStarConfig("SunTestService");
            Config.SunData.OverlayExistingData = false;

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var firstResult = await dr.GetSunDataAsync(l);
            Assert.IsNotNull(firstResult);

            firstResult.Pulled = DateTimeOffset.Now.AddHours(-12);

            sunDataToReturn = refreshedSunData;

            var secondResult = await dr.GetSunDataAsync(l);
            Assert.IsNotNull(secondResult);

            Assert.AreEqual(1, secondResult.Lines.Count(), "Expected exactly one SunLine after replace.");
            Assert.AreEqual(DateTime.Today.AddDays(1), secondResult.Lines.First().For, "Expected replaced SunLine to have the refreshed date.");

            sunService.Verify(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Exactly(2));
            Assert.AreEqual(2, SunDataUpdated.Count, "Expected SunDataUpdated event to fire twice.");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldMergeDataWithNewOnRefreshIfOverlayIsTrue()
        {
            var sunService = SetupSunDataService("SunTestService");

            var initialSunLine = GenerateSunLine(forDatetime: DateTime.Today);
            var initialSunData = new SunData(DateTimeOffset.Now, initialSunLine);

            var refreshedSunLine = GenerateSunLine(forDatetime: DateTime.Today.AddDays(1));
            var refreshedSunData = new SunData(DateTimeOffset.Now, refreshedSunLine);

            var sunDataToReturn = initialSunData;
            sunService.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(() => sunDataToReturn);

            Config.SunData = GenerateAllStarConfig("SunTestService");
            Config.SunData.OverlayExistingData = true;

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var firstResult = await dr.GetSunDataAsync(l);
            Assert.IsNotNull(firstResult);

            firstResult.Pulled = DateTimeOffset.Now.AddHours(-12);

            sunDataToReturn = refreshedSunData;

            var secondResult = await dr.GetSunDataAsync(l);
            Assert.IsNotNull(secondResult);

            var lines = secondResult.Lines.ToList();

            Assert.AreEqual(2, lines.Count, $"Expected both initial and refreshed SunLines to be present due to overlay: {JsonSerializer.Serialize(secondResult)}");
            Assert.IsTrue(lines.Any(sl => sl.For == DateTime.Today), "Initial SunLine should remain.");
            Assert.IsTrue(lines.Any(sl => sl.For == DateTime.Today.AddDays(1)), "Refreshed SunLine should be added.");

            sunService.Verify(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Exactly(2));
            Assert.AreEqual(2, SunDataUpdated.Count, "Expected SunDataUpdated event to fire twice.");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldRaiseSunDataUpdatedEventOnSuccessfulRetrieval()
        {
            var sunService = SetupSunDataService("SunTestService");

            var sunLine = GenerateSunLine();
            var sunData = new SunData(DateTimeOffset.Now, sunLine);

            sunService.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(sunData);

            Config.SunData = GenerateAllStarConfig("SunTestService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetSunDataAsync(l);

            Assert.IsNotNull(result, "Expected SunData to be returned.");
            Assert.AreEqual(1, SunDataUpdated.Count, "Expected SunDataUpdated event to fire once.");
            Assert.AreEqual(l, SunDataUpdated.First().Location, "SunDataUpdated event should have the correct location.");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldThrowWhenServiceThrowsException()
        {
            var sunService = SetupSunDataService("SunTestService");

            sunService.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ThrowsAsync(new InvalidOperationException("Simulated service failure."));

            Config.SunData = GenerateAllStarConfig("SunTestService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => dr.GetSunDataAsync(l));

            Assert.AreEqual("Simulated service failure.", ex.Message, "Expected exception message to match.");
            Assert.AreEqual(0, SunDataUpdated.Count, "SunDataUpdated event should not fire when exception is thrown.");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldNotThrowIfSunLineHasNullMoonData()
        {
            var sunService = SetupSunDataService("SunTestService");

            var sunLine = GenerateSunLine();
            sunLine.MoonData = null;

            var sunData = new SunData(DateTimeOffset.Now, sunLine);

            sunService.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(sunData);

            Config.SunData = GenerateAllStarConfig("SunTestService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetSunDataAsync(l);

            Assert.IsNotNull(result, "Expected SunData to be returned even if SunLine.MoonData is null.");
            Assert.IsTrue(result.Lines.Any(), "Expected SunData to contain at least one SunLine.");
            Assert.IsTrue(result.Lines.All(sl => sl.MoonData != null), "Expected MoonData to be generated/initialized for SunLines.");

            Assert.AreEqual(1, SunDataUpdated.Count, "Expected SunDataUpdated event to fire once.");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldHandleMultipleServicesReturningDataForSameDate()
        {
            var sunService1 = SetupSunDataService("SunService1");
            var sunService2 = SetupSunDataService("SunService2");

            var commonDate = DateTime.Today;

            var sunLine1 = GenerateSunLine(forDatetime: commonDate);
            var sunLine2 = GenerateSunLine(forDatetime: commonDate);

            var sunData1 = new SunData(DateTimeOffset.Now, sunLine1);
            var sunData2 = new SunData(DateTimeOffset.Now, sunLine2);

            sunService1.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(sunData1);

            sunService2.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(sunData2);

            Config.SunData = GenerateAllStarConfig("SunService1", "SunService2");
            Config.SunData.OverlayExistingData = true;

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetSunDataAsync(l);

            Assert.IsNotNull(result, "Expected SunData to be returned.");
            Assert.AreEqual(1, result.Lines.Count(l => l.For.Date == commonDate), "Expected only one SunLine for the shared date after overlay/merge.");
            Assert.AreEqual(1, SunDataUpdated.Count, "Expected SunDataUpdated event to fire once.");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldOverlayMoonDataCorrectly()
        {
            var sunService1 = SetupSunDataService("SunService1");
            var sunService2 = SetupSunDataService("SunService2");

            var commonDate = DateTime.Today;

            var moonData1 = GenerateFullyFormedMoonData(forDatetime: commonDate, moonPhase: MoonPhaseEnum.FullMoon);
            var moonData2 = GenerateFullyFormedMoonData(forDatetime: commonDate, moonPhase: MoonPhaseEnum.NewMoon);

            var sunLine1 = GenerateSunLine(forDatetime: commonDate, moonData: moonData1);
            var sunLine2 = GenerateSunLine(forDatetime: commonDate, moonData: moonData2);

            var sunData1 = new SunData(DateTimeOffset.Now, sunLine1);
            var sunData2 = new SunData(DateTimeOffset.Now, sunLine2);

            sunService1.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(sunData1);

            sunService2.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(sunData2);

            Config.SunData = GenerateAllStarConfig("SunService1", "SunService2");
            Config.SunData.OverlayExistingData = true;

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetSunDataAsync(l);

            Assert.IsNotNull(result, "Expected SunData to be returned.");
            var moonData = result.Lines.First(l => l.For.Date == commonDate).MoonData;
            Assert.IsNotNull(moonData, "Expected MoonData to be present after overlay.");
            Assert.IsTrue(Enum.IsDefined(typeof(MoonPhaseEnum), moonData.MoonPhase), "Expected MoonPhase to be set after overlay.");
            Assert.AreEqual(1, SunDataUpdated.Count, "Expected SunDataUpdated event to fire once.");
        }

        private async Task TestMoonPropertyWeighting(string elementName, double minValue, double maxValue, Action<MoonData, double> setValue, Func<MoonData, double?> getValue, int? service1Weight = null, int? service2Weight = null)
        {
            service1Weight = service1Weight ?? TestHelpers.RandomIntBetween(100, 500);
            service2Weight = service2Weight ?? TestHelpers.RandomIntBetween(100, 500);

            var sunService1Name = TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters);
            var sunService2Name = TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters);

            var sunService1 = SetupSunDataService(sunService1Name);
            var sunService2 = SetupSunDataService(sunService2Name);

            var commonDate = DateTime.Today;

            var value1 = TestHelpers.RandomDoubleBetween(minValue, maxValue);
            var value2 = TestHelpers.RandomDoubleBetween(minValue, maxValue);

            var moonData1 = GenerateMoonData();
            var moonData2 = GenerateMoonData();

            setValue(moonData1, value1);
            setValue(moonData2, value2);

            var sunLine1 = GenerateSunLine(forDatetime: commonDate, moonData: moonData1);
            var sunLine2 = GenerateSunLine(forDatetime: commonDate, moonData: moonData2);

            var sunData1 = new SunData(DateTimeOffset.Now, sunLine1);
            var sunData2 = new SunData(DateTimeOffset.Now, sunLine2);

            sunService1.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(sunData1);

            sunService2.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(sunData2);

            Config.SunData = new DataConfig()
            {
                OverlayExistingData = true,
                Elements = [
                    new ElementConfig
                    {
                        Name = elementName,
                        ServiceElements = [
                            new ServiceElementConfig() { ServiceName = sunService1Name, Action = "Average", Weight = service1Weight.Value },
                            new ServiceElementConfig() { ServiceName = sunService2Name, Action = "Average", Weight = service2Weight.Value}
                        ]
                    }
                ]
            };

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetSunDataAsync(l);

            Assert.IsNotNull(result, "Expected SunData to be returned.");
            Assert.IsNotNull(result.Lines, "Expected lines to exist.");
            Assert.IsTrue(result.Lines.Any(), "Expected lines to have at least one line.");
            Assert.IsNotNull(result.Lines.First().MoonData, "Expected MoonData to be set.");
            var val = getValue(result.Lines.First().MoonData!);
            Assert.IsNotNull(val, $"Expected {elementName} to be set.");

            double expectedWeighted = ((value1 * service1Weight.Value) + (value2 * service2Weight.Value)) / (double)(service1Weight.Value + service2Weight.Value);
            Assert.AreEqual(expectedWeighted, val.Value, 0.01, $"Expected weighted average to be {expectedWeighted} but was {val}.");
        }

        private async Task TestMoonPropertyWeighting(string elementName, DateTimeOffset minValue, DateTimeOffset maxValue, Action<MoonData, DateTimeOffset> setValue, Func<MoonData, DateTimeOffset?> getValue, int? service1Weight = null, int? service2Weight = null)
        {
            service1Weight = service1Weight ?? TestHelpers.RandomIntBetween(100, 500);
            service2Weight = service2Weight ?? TestHelpers.RandomIntBetween(100, 500);

            var sunService1Name = TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters);
            var sunService2Name = TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters);

            var sunService1 = SetupSunDataService(sunService1Name);
            var sunService2 = SetupSunDataService(sunService2Name);

            var commonDate = DateTime.Today;

            var value1 = TestHelpers.RandomDateTimeOffsetBetween(minValue, maxValue);
            var value2 = TestHelpers.RandomDateTimeOffsetBetween(minValue, maxValue);

            var moonData1 = GenerateMoonData();
            var moonData2 = GenerateMoonData();

            setValue(moonData1, value1);
            setValue(moonData2, value2);

            var sunLine1 = GenerateSunLine(forDatetime: commonDate, moonData: moonData1);
            var sunLine2 = GenerateSunLine(forDatetime: commonDate, moonData: moonData2);

            var sunData1 = new SunData(DateTimeOffset.Now, sunLine1);
            var sunData2 = new SunData(DateTimeOffset.Now, sunLine2);

            sunService1.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(sunData1);

            sunService2.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(sunData2);

            Config.SunData = new DataConfig()
            {
                OverlayExistingData = true,
                Elements = [
                    new ElementConfig
                    {
                        Name = elementName,
                        ServiceElements = [
                            new ServiceElementConfig() { ServiceName = sunService1Name, Action = "Average", Weight = service1Weight.Value },
                            new ServiceElementConfig() { ServiceName = sunService2Name, Action = "Average", Weight = service2Weight.Value}
                        ]
                    }
                ]
            };

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetSunDataAsync(l);

            Assert.IsNotNull(result, $"Expected {nameof(SunData)} to be returned.");
            Assert.IsNotNull(result.Lines, "Expected lines to exist.");
            Assert.IsTrue(result.Lines.Any(), "Expected lines to have at least one line.");
            var lin = result.Lines.First();
            Assert.IsNotNull(lin.MoonData, $"Expected {nameof(MoonData)} to be set.");
            var val = getValue(lin.MoonData);
            Assert.IsNotNull(val, $"Expected {elementName} to be set.");

            long minTicks = value1.UtcTicks;
            long maxTicks = value2.UtcTicks;
            long separatorTicks = maxTicks - minTicks;

            double expectedWeighted = ((separatorTicks * service2Weight.Value)) / (double)(service1Weight.Value + service2Weight.Value);
            var expectedTime = value1.AddTicks((long)expectedWeighted);
            var testedTicks = val?.UtcTicks;
            var expectedTicks = expectedTime.UtcTicks;
            var expectedMaximumDelta = 60 * TimeSpan.TicksPerSecond;
            var delta = testedTicks - expectedTicks;
            if (delta < 0) delta = delta * -1;
            Assert.IsTrue(delta < expectedMaximumDelta, $"{Environment.NewLine}Expected weighted average to be {expectedTime} but was {val}." +
                $"{Environment.NewLine} MinDate: {minValue}" +
                $"{Environment.NewLine} MaxDate: {maxValue}" +
                $"{Environment.NewLine} value1: {value1}" +
                $"{Environment.NewLine} value2: {value2}" +
                $"{Environment.NewLine} service1Weight: {service1Weight}" +
                $"{Environment.NewLine} service2Weight: {service2Weight}");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldWeightMoonRiseCorrectly()
        {
            var moonRise = DateTimeOffset.Now.DayOf().AddHours(TestHelpers.RandomIntBetween(1, 4));
            var moonSet = moonRise.AddHours(TestHelpers.RandomIntBetween(5, 10));
            await TestMoonPropertyWeighting($"{nameof(MoonData)}.{nameof(MoonData.Moonrise)}", moonRise, moonSet, (md, dto) => md.Moonrise = dto, md => md.Moonrise);
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldWeightMoonDeclinationCorrectly()
        {
            await TestMoonPropertyWeighting("MoonData.MoonDeclination", 5, 100, (md, v) => md.MoonDeclination = new MoonProperty(v, DateTimeOffset.Now), md => md.MoonDeclination?.Value);
            return;
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldWeightMoonAzimuthCorrectly()
        {
            await TestMoonPropertyWeighting(
                "MoonData.MoonAzimuth",
                0, 360,
                (md, v) => md.MoonAzimuth = new MoonProperty(v, DateTimeOffset.Now),
                md => md.MoonAzimuth?.Value
            );
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldWeightMoonParallacticAngleCorrectly()
        {
            await TestMoonPropertyWeighting(
                "MoonData.MoonParallacticAngle",
                0, 360,
                (md, v) => md.MoonParallacticAngle = new MoonProperty(v, DateTimeOffset.Now),
                md => md.MoonParallacticAngle?.Value
            );
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldWeightMoonDistanceCorrectly()
        {
            await TestMoonPropertyWeighting(
                "MoonData.MoonDistance",
                100_000, 600_000,
                (md, v) => md.MoonDistance = new MoonProperty(v, DateTimeOffset.Now),
                md => md.MoonDistance?.Value
            );
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldWeightMoonAltitudeCorrectly()
        {
            await TestMoonPropertyWeighting(
                "MoonData.MoonAltitude",
                -90, 90,
                (md, v) => md.MoonAltitude = new MoonProperty(v, DateTimeOffset.Now),
                md => md.MoonAltitude?.Value
            );
        }

        [TestMethod]
        public void GetWeightedAverage_ShouldReturnDateTimeOffsetWithinRangeForRandomValues()
        {
            var dr = GetDataRetriever();

            var baseDate = DateTimeOffset.UtcNow;
            var hoursApart = Random.Shared.Next(1, 6);

            var date1 = baseDate;
            var date2 = baseDate.AddHours(hoursApart);

            if (Random.Shared.Next(0, 2) == 0)
            {
                var temp = date1;
                date1 = date2;
                date2 = temp;
            }

            var weight1 = Random.Shared.Next(100, 501);
            var weight2 = Random.Shared.Next(100, 501);

            Assert.IsTrue(weight1 > 0 && weight2 > 0, $"Invalid weights: {weight1}, {weight2}");
            Assert.IsTrue(date1 != DateTimeOffset.MinValue, "date1 is MinValue");
            Assert.IsTrue(date2 != DateTimeOffset.MinValue, "date2 is MinValue");

            var tuples = new List<Tuple<int, DateTimeOffset>>
            {
                new Tuple<int, DateTimeOffset>(weight1, date1),
                new Tuple<int, DateTimeOffset>(weight2, date2)
            };

            var result = dr.InvokePrivateGenericMethod<DateTimeOffset>(
                "GetWeightedAverage",
                new[] { typeof(IEnumerable<Tuple<int, DateTimeOffset>>) },
                tuples
            );

            var minDate = date1 < date2 ? date1 : date2;
            var maxDate = date1 > date2 ? date1 : date2;

            Assert.IsTrue(result.UtcDateTime >= minDate.UtcDateTime && result.UtcDateTime <= maxDate.UtcDateTime,
                $"Expected weighted average between {minDate.UtcDateTime} and {maxDate.UtcDateTime}, but got {result.UtcDateTime}. Weights were {weight1} and {weight2}.");
        }

        private async Task TestSunPropertyWeighting(string elementName, DateTimeOffset minValue, DateTimeOffset maxValue, Action<SunLine, DateTimeOffset> setValue, Func<SunLine, DateTimeOffset?> getValue, int? service1Weight = null, int? service2Weight = null)
        {
            service1Weight = service1Weight ?? TestHelpers.RandomIntBetween(100, 500);
            service2Weight = service2Weight ?? TestHelpers.RandomIntBetween(100, 500);

            var sunService1Name = TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters);
            var sunService2Name = TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters);

            var sunService1 = SetupSunDataService(sunService1Name);
            var sunService2 = SetupSunDataService(sunService2Name);

            var commonDate = DateTime.Today;

            var sunLine1 = GenerateSunLine();
            var sunLine2 = GenerateSunLine();

            setValue(sunLine1, minValue);
            setValue(sunLine2, maxValue);

            var sunData1 = new SunData(DateTimeOffset.Now, sunLine1);
            var sunData2 = new SunData(DateTimeOffset.Now, sunLine2);

            sunService1.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(sunData1);

            sunService2.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(sunData2);

            Config.SunData = new DataConfig()
            {
                OverlayExistingData = true,
                Elements = [
                    new ElementConfig
                    {
                        Name = elementName,
                        ServiceElements = [
                            new ServiceElementConfig() { ServiceName = sunService1Name, Action = "Average", Weight = service1Weight.Value },
                            new ServiceElementConfig() { ServiceName = sunService2Name, Action = "Average", Weight = service2Weight.Value}
                        ]
                    }
                ]
            };

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetSunDataAsync(l);

            Assert.IsNotNull(result, "Expected SunData to be returned.");
            Assert.IsNotNull(result.Lines, "Expected lines to exist.");
            Assert.IsTrue(result.Lines.Any(), "Expected lines to have at least one line.");
            var val = getValue(result.Lines.First());
            Assert.IsNotNull(val, $"Expected {elementName} to be set.");

            double ticks1 = 0;
            double ticks2 = (maxValue.UtcTicks - minValue.UtcTicks);
            double expectedWeighted = ((ticks1 * service1Weight.Value) + (ticks2 * service2Weight.Value)) / (double)(service1Weight.Value + service2Weight.Value);
            var timeExpected = minValue.AddTicks((long)expectedWeighted);
            Assert.AreEqual(timeExpected.UtcTicks, val.Value.UtcTicks, 100000, $"Weight was {service1Weight},{service2Weight} between {minValue} and {maxValue} with expected weighted average to be {timeExpected} but was {val}.");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldWeightAstronomicalTwilightBegin()
        {
            await TestSunPropertyWeighting(
                "AstronomicalTwilightBegin",
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(-120, -60)),
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(60, 120)),
                (sl, t) => sl.AstronomicalTwilightBegin = t,
                sl => sl.AstronomicalTwilightBegin);
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldWeightNauticalTwilightBegin()
        {
            await TestSunPropertyWeighting(
                "NauticalTwilightBegin",
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(-120, -60)),
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(60, 120)),
                (sl, t) => sl.NauticalTwilightBegin = t,
                sl => sl.NauticalTwilightBegin);
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldWeightCivilTwilightBegin()
        {
            await TestSunPropertyWeighting(
                "CivilTwilightBegin",
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(-120, -60)),
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(60, 120)),
                (sl, t) => sl.CivilTwilightBegin = t,
                sl => sl.CivilTwilightBegin);
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldWeightSunrise()
        {
            await TestSunPropertyWeighting(
                "Sunrise",
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(-120, -60)),
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(60, 120)),
                (sl, t) => sl.Sunrise = t,
                sl => sl.Sunrise);
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldWeightDayStart()
        {
            await TestSunPropertyWeighting(
                "DayStart",
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(-120, -60)),
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(60, 120)),
                (sl, t) => sl.DayStart = t,
                sl => sl.DayStart);
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldWeightSolarNoon()
        {
            await TestSunPropertyWeighting(
                "SolarNoon",
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(-120, -60)),
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(60, 120)),
                (sl, t) => sl.SolarNoon = t,
                sl => sl.SolarNoon);
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldWeightDayEnd()
        {
            await TestSunPropertyWeighting(
                "DayEnd",
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(-120, -60)),
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(60, 120)),
                (sl, t) => sl.DayEnd = t,
                sl => sl.DayEnd);
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldWeightSunset()
        {
            await TestSunPropertyWeighting(
                "Sunset",
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(-120, -60)),
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(60, 120)),
                (sl, t) => sl.Sunset = t,
                sl => sl.Sunset);
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldWeightCivilTwilightEnd()
        {
            await TestSunPropertyWeighting(
                "CivilTwilightEnd",
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(-120, -60)),
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(60, 120)),
                (sl, t) => sl.CivilTwilightEnd = t,
                sl => sl.CivilTwilightEnd);
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldWeightNauticalTwilightEnd()
        {
            await TestSunPropertyWeighting(
                "NauticalTwilightEnd",
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(-120, -60)),
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(60, 120)),
                (sl, t) => sl.NauticalTwilightEnd = t,
                sl => sl.NauticalTwilightEnd);
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldWeightAstronomicalTwilightEnd()
        {
            await TestSunPropertyWeighting(
                "AstronomicalTwilightEnd",
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(-120, -60)),
                DateTimeOffset.Now.AddMinutes(TestHelpers.RandomIntBetween(60, 120)),
                (sl, t) => sl.AstronomicalTwilightEnd = t,
                sl => sl.AstronomicalTwilightEnd);
        }
    }
}
