using ID.WeatherDashboard.API.Codes;
using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace ID.WeatherDashboard.APITests.Services
{
    [TestClass]
    public class DataRetrieverServiceTests_Current : DataRetrieverServiceTests
    {
        private Mock<ICurrentQueryService> SetupCurrentQueryService(string name)
        {
            var s = new Mock<ICurrentQueryService>();
            s.SetupGet(m => m.ServiceName).Returns(name);
            CurrentQueryServiceMocks.Add(s);
            return s;
        }

        private Mock<ISunDataService> SetupSunDataService(string name)
        {
            var s = new Mock<ISunDataService>();
            s.SetupGet(m => m.ServiceName).Returns(name);
            SunDataServiceMocks.Add(s);
            return s;
        }

        private Mock<IAlertQueryService> SetupAlertQueryService(string name)
        {
            var s = new Mock<IAlertQueryService>();
            s.SetupGet(m => m.ServiceName).Returns(name);
            AlertQueryServiceMocks.Add(s);
            return s;
        }

        private Mock<IHistoryQueryService> SetupHistoryQueryService(string name)
        {
            var s = new Mock<IHistoryQueryService>();
            s.SetupGet(m => m.ServiceName).Returns(name);
            HistoryQueryServiceMocks.Add(s);
            return s;
        }

        private void SetupDefaultSun()
        {
            var sun = SetupSunDataService("DefaultSun");
            sun.Setup(s => s.GetSunDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new SunData(DateTimeOffset.Now));
            Config.SunData = GenerateAllStarConfig("DefaultSun");
        }

        private void SetupDefaultAlert()
        {
            var alert = SetupAlertQueryService("DefaultAlert");
            alert.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>()))
                .ReturnsAsync(new AlertData(DateTimeOffset.Now));
            Config.AlertData = GenerateAllStarConfig("DefaultAlert");
        }

        private void SetupDefaultHistory()
        {
            var hist = SetupHistoryQueryService("DefaultHistory");
            hist.Setup(h => h.GetHistoryDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new HistoryData(DateTimeOffset.Now));
            Config.HistoryData = GenerateAllStarConfig("DefaultHistory");
        }

        private void SetupDefaults()
        {
            SetupDefaultSun();
            SetupDefaultAlert();
            SetupDefaultHistory();
        }

        private CurrentData GenerateCurrentData(
            DateTimeOffset? pulled = null,
            string[]? sources = null,
            DateTimeOffset? observed = null,
            WindDirection? windDirection = null,
            float? humidity = null,
            Temperature? currentTemperature = null,
            Temperature? feelsLike = null,
            Temperature? heatIndex = null,
            Temperature? dewPoint = null,
            float? uvIndex = null,
            Pressure? pressure = null,
            Coordinates? coordinates = null,
            WeatherConditions? weatherConditions = null)
        {
            return new CurrentData(pulled ?? DateTimeOffset.Now, sources ?? [])
            {
                Observed = observed ?? DateTimeOffset.Now,
                WindDirection = windDirection,
                Humidity = humidity,
                CurrentTemperature = currentTemperature,
                FeelsLike = feelsLike,
                HeatIndex = heatIndex,
                DewPoint = dewPoint,
                UVIndex = uvIndex,
                Pressure = pressure,
                Coordinates = coordinates,
                WeatherConditions = weatherConditions ?? GenerateWeatherConditions()
            };
        }

        private CurrentData GenerateFullyFormedCurrentData(
            DateTimeOffset? pulled = null,
            string[]? sources = null,
            DateTimeOffset? observed = null)
        {
            return GenerateCurrentData(
                pulled,
                sources,
                observed,
                windDirection: new WindDirection(TestHelpers.RandomFloatBetween(0,360)),
                humidity: TestHelpers.RandomFloatBetween(0,100),
                currentTemperature: new Temperature(TestHelpers.RandomFloatBetween(30,100)),
                feelsLike: new Temperature(TestHelpers.RandomFloatBetween(30,100)),
                heatIndex: new Temperature(TestHelpers.RandomFloatBetween(30,110)),
                dewPoint: new Temperature(TestHelpers.RandomFloatBetween(10,70)),
                uvIndex: TestHelpers.RandomFloatBetween(0,12),
                pressure: new Pressure(TestHelpers.RandomFloatBetween(950,1050), PressureEnum.Millibars),
                coordinates: new Coordinates(TestHelpers.RandomDoubleBetween(-90,90), TestHelpers.RandomDoubleBetween(-180,180)),
                weatherConditions: GenerateFullyFormedWeatherConditions());
        }

        private async Task TestCurrentFloatWeighting(string elementName, Action<CurrentData,float> setValue, Func<CurrentData,float?> getValue)
        {
            SetupDefaults();
            var weight1 = TestHelpers.RandomIntBetween(100,500);
            var weight2 = TestHelpers.RandomIntBetween(100,500);
            var service1Name = TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters);
            var service2Name = TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters);
            var service1 = SetupCurrentQueryService(service1Name);
            var service2 = SetupCurrentQueryService(service2Name);
            var value1 = TestHelpers.RandomFloatBetween(0,100);
            var value2 = TestHelpers.RandomFloatBetween(0,100);
            var data1 = GenerateFullyFormedCurrentData();
            var data2 = GenerateFullyFormedCurrentData();
            setValue(data1,value1);
            setValue(data2,value2);
            service1.Setup(s=>s.GetCurrentDataAsync(It.IsAny<Location>())).ReturnsAsync(data1);
            service2.Setup(s=>s.GetCurrentDataAsync(It.IsAny<Location>())).ReturnsAsync(data2);

            // ensure other data configs are present
            Config.SunData = GenerateAllStarConfig("DefaultSun");
            Config.AlertData = GenerateAllStarConfig("DefaultAlert");
            Config.HistoryData = GenerateAllStarConfig("DefaultHistory");

            Config.CurrentData = new DataConfig
            {
                OverlayExistingData = true,
                Elements = [ new ElementConfig
                    {
                        Name = elementName,
                        ServiceElements = [
                            new ServiceElementConfig{ ServiceName = service1Name, Action = "Average", Weight = weight1 },
                            new ServiceElementConfig{ ServiceName = service2Name, Action = "Average", Weight = weight2 }
                        ]
                    }
                ]
            };

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");
            var result = await dr.GetCurrentDataAsync(l);
            Assert.IsNotNull(result);
            var val = getValue(result);
            Assert.IsNotNull(val);
            var expected = ((value1*weight1)+(value2*weight2))/(float)(weight1+weight2);
            Assert.AreEqual(expected, val!.Value, 0.01);
        }

        private async Task TestCurrentTemperatureWeighting(string elementName, Action<CurrentData,Temperature> setValue, Func<CurrentData,Temperature?> getValue)
        {
            SetupDefaults();
            var weight1 = TestHelpers.RandomIntBetween(100,500);
            var weight2 = TestHelpers.RandomIntBetween(100,500);
            var service1Name = TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters);
            var service2Name = TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters);
            var service1 = SetupCurrentQueryService(service1Name);
            var service2 = SetupCurrentQueryService(service2Name);
            var value1 = TestHelpers.RandomFloatBetween(40,100);
            var value2 = TestHelpers.RandomFloatBetween(40,100);
            var data1 = GenerateFullyFormedCurrentData();
            var data2 = GenerateFullyFormedCurrentData();
            setValue(data1,new Temperature(value1));
            setValue(data2,new Temperature(value2));
            service1.Setup(s=>s.GetCurrentDataAsync(It.IsAny<Location>())).ReturnsAsync(data1);
            service2.Setup(s=>s.GetCurrentDataAsync(It.IsAny<Location>())).ReturnsAsync(data2);

            Config.SunData = GenerateAllStarConfig("DefaultSun");
            Config.AlertData = GenerateAllStarConfig("DefaultAlert");
            Config.HistoryData = GenerateAllStarConfig("DefaultHistory");

            Config.CurrentData = new DataConfig
            {
                OverlayExistingData = true,
                Elements = [ new ElementConfig
                    {
                        Name = elementName,
                        ServiceElements = [
                            new ServiceElementConfig{ ServiceName = service1Name, Action = "Average", Weight = weight1 },
                            new ServiceElementConfig{ ServiceName = service2Name, Action = "Average", Weight = weight2 }
                        ]
                    }
                ]
            };

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");
            var result = await dr.GetCurrentDataAsync(l);
            Assert.IsNotNull(result);
            var temp = getValue(result);
            Assert.IsNotNull(temp);
            var expected = ((value1*weight1)+(value2*weight2))/(float)(weight1+weight2);
            Assert.AreEqual(expected, temp!.To(TemperatureEnum.Fahrenheit)!.Value, 0.01);
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldErrorIfNoServicesConfigured()
        {
            SetupDefaults();
            var dr = GetDataRetriever();
            var l = new Location("TestLocation");
            Config.CurrentData = GenerateAllStarConfig("CurrentService");
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => dr.GetCurrentDataAsync(l));
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldErrorIfNoConfigProvided()
        {
            var service = SetupCurrentQueryService("CurrentService");
            SetupDefaults();
            var dr = GetDataRetriever();
            var l = new Location("TestLocation");
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => dr.GetCurrentDataAsync(l));
            service.Verify(s=>s.GetCurrentDataAsync(It.IsAny<Location>()), Times.Never());
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldErrorIfServiceNotRegistered()
        {
            SetupCurrentQueryService("Service1");
            SetupDefaults();
            Config.CurrentData = GenerateAllStarConfig("Service1","Service2");
            var dr = GetDataRetriever();
            var l = new Location("TestLocation");
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => dr.GetCurrentDataAsync(l));
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldErrorIfAllServicesReturnNull()
        {
            var s1 = SetupCurrentQueryService("Service1");
            var s2 = SetupCurrentQueryService("Service2");
            s1.Setup(s=>s.GetCurrentDataAsync(It.IsAny<Location>())).ReturnsAsync((CurrentData?)null);
            s2.Setup(s=>s.GetCurrentDataAsync(It.IsAny<Location>())).ReturnsAsync((CurrentData?)null);
            SetupDefaults();
            Config.CurrentData = GenerateAllStarConfig("Service1","Service2");
            var dr = GetDataRetriever();
            var l = new Location("TestLocation");
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => dr.GetCurrentDataAsync(l));
            s1.Verify(s=>s.GetCurrentDataAsync(It.IsAny<Location>()), Times.Once());
            s2.Verify(s=>s.GetCurrentDataAsync(It.IsAny<Location>()), Times.Once());
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldReturnCurrentDataWhenServiceReturnsData()
        {
            var service = SetupCurrentQueryService("Service1");
            var cd = GenerateCurrentData();
            service.Setup(s=>s.GetCurrentDataAsync(It.IsAny<Location>())).ReturnsAsync(cd);
            SetupDefaults();
            Config.CurrentData = GenerateAllStarConfig("Service1");
            var dr = GetDataRetriever();
            var l = new Location("TestLocation");
            var result = await dr.GetCurrentDataAsync(l);
            Assert.IsNotNull(result);
            Assert.AreEqual(cd.Observed, result.Observed);
            Assert.AreEqual(1, CurrentDataUpdated.Count);
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldMergeResultsFromMultipleServicesWhenOverlayTrue()
        {
            var s1 = SetupCurrentQueryService("Service1");
            var s2 = SetupCurrentQueryService("Service2");
            var d1 = GenerateCurrentData(humidity:55);
            var d2 = GenerateCurrentData(currentTemperature:new Temperature(70));
            s1.Setup(s=>s.GetCurrentDataAsync(It.IsAny<Location>())).ReturnsAsync(d1);
            s2.Setup(s=>s.GetCurrentDataAsync(It.IsAny<Location>())).ReturnsAsync(d2);
            SetupDefaults();
            Config.CurrentData = GenerateAllStarConfig("Service1","Service2");
            Config.CurrentData.OverlayExistingData = true;
            var dr = GetDataRetriever();
            var l = new Location("TestLocation");
            var result = await dr.GetCurrentDataAsync(l);
            Assert.IsNotNull(result);
            Assert.AreEqual(55, result.Humidity);
            Assert.IsNotNull(result.CurrentTemperature, $"Result current temperature did not output! Result Output: {JsonSerializer.Serialize(result)}");
            Assert.AreEqual(70, result.CurrentTemperature!.To(TemperatureEnum.Fahrenheit));
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldRaiseCurrentDataUpdatedEvent()
        {
            var service = SetupCurrentQueryService("Service1");
            service.Setup(s=>s.GetCurrentDataAsync(It.IsAny<Location>())).ReturnsAsync(GenerateCurrentData());
            SetupDefaults();
            Config.CurrentData = GenerateAllStarConfig("Service1");
            var dr = GetDataRetriever();
            var l = new Location("TestLocation");
            var result = await dr.GetCurrentDataAsync(l);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, CurrentDataUpdated.Count);
            Assert.AreEqual(l, CurrentDataUpdated.First().Location);
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldReturnCachedDataIfNotExpired()
        {
            var service = SetupCurrentQueryService("Service1");
            var data = GenerateCurrentData();
            service.Setup(s=>s.GetCurrentDataAsync(It.IsAny<Location>())).ReturnsAsync(data);
            SetupDefaults();
            Config.CurrentData = GenerateAllStarConfig("Service1");
            var dr = GetDataRetriever();
            var l = new Location("TestLocation");
            var first = await dr.GetCurrentDataAsync(l);
            var second = await dr.GetCurrentDataAsync(l);
            Assert.AreSame(first, second);
            service.Verify(s=>s.GetCurrentDataAsync(It.IsAny<Location>()), Times.Once());
            Assert.AreEqual(1, CurrentDataUpdated.Count);
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldRefreshDataIfExpiredBasedOnMaxDataAge()
        {
            var service = SetupCurrentQueryService("Service1");
            var initial = GenerateCurrentData();
            var refreshed = GenerateCurrentData(observed: DateTimeOffset.Now.AddMinutes(1));
            var current = initial;
            service.Setup(s=>s.GetCurrentDataAsync(It.IsAny<Location>())).ReturnsAsync(() => current);
            SetupDefaults();
            Config.CurrentData = GenerateAllStarConfig("Service1");
            var dr = GetDataRetriever();
            var l = new Location("TestLocation");
            var first = await dr.GetCurrentDataAsync(l);
            Assert.IsNotNull(first);
            first.Pulled = DateTimeOffset.Now.AddMinutes(-10);
            current = refreshed;
            var second = await dr.GetCurrentDataAsync(l);
            Assert.IsNotNull(second);
            Assert.AreEqual(refreshed.Observed, second.Observed);
            service.Verify(s=>s.GetCurrentDataAsync(It.IsAny<Location>()), Times.Exactly(2));
            Assert.AreEqual(2, CurrentDataUpdated.Count);
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldRefreshDataIfRefreshEventNextHour()
        {
            var service = SetupCurrentQueryService("Service1");
            var initial = GenerateCurrentData();
            var refreshed = GenerateCurrentData(observed: DateTimeOffset.Now.AddMinutes(1));
            var current = initial;
            service.Setup(s=>s.GetCurrentDataAsync(It.IsAny<Location>())).ReturnsAsync(() => current);
            SetupDefaults();
            Config.CurrentData = GenerateAllStarConfig("Service1");
            Config.CurrentData.RefreshEvent = "NextHour";
            var dr = GetDataRetriever();
            var l = new Location("TestLocation");
            var first = await dr.GetCurrentDataAsync(l);
            Assert.IsNotNull(first);
            first.Pulled = DateTimeOffset.Now.AddHours(-2);
            current = refreshed;
            var second = await dr.GetCurrentDataAsync(l);
            Assert.IsNotNull(second);
            Assert.AreEqual(refreshed.Observed, second.Observed);
            service.Verify(s=>s.GetCurrentDataAsync(It.IsAny<Location>()), Times.Exactly(2));
            Assert.AreEqual(2, CurrentDataUpdated.Count);
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldOverlayDataWhenOverlayExistingDataTrue()
        {
            var service = SetupCurrentQueryService("Service1");
            var firstData = GenerateCurrentData(humidity:40);
            var refreshData = GenerateCurrentData(currentTemperature:new Temperature(75));
            var current = firstData;
            service.Setup(s=>s.GetCurrentDataAsync(It.IsAny<Location>())).ReturnsAsync(() => current);
            SetupDefaults();
            Config.CurrentData = GenerateAllStarConfig("Service1");
            Config.CurrentData.OverlayExistingData = true;
            var dr = GetDataRetriever();
            var l = new Location("TestLocation");
            var first = await dr.GetCurrentDataAsync(l);
            Assert.IsNotNull(first);
            first.Pulled = DateTimeOffset.Now.AddMinutes(-10);
            current = refreshData;
            var second = await dr.GetCurrentDataAsync(l);
            Assert.IsNotNull(second);
            Assert.AreEqual(40, second.Humidity);
            Assert.IsNotNull(second.CurrentTemperature);
            Assert.AreEqual(75, second.CurrentTemperature!.To(TemperatureEnum.Fahrenheit));
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldReplaceDataWhenOverlayExistingDataFalse()
        {
            var service = SetupCurrentQueryService("Service1");
            var firstData = GenerateCurrentData(humidity:40);
            var refreshData = GenerateCurrentData(currentTemperature:new Temperature(75));
            var current = firstData;
            service.Setup(s=>s.GetCurrentDataAsync(It.IsAny<Location>())).ReturnsAsync(() => current);
            SetupDefaults();
            Config.CurrentData = GenerateAllStarConfig("Service1");
            Config.CurrentData.OverlayExistingData = false;
            var dr = GetDataRetriever();
            var l = new Location("TestLocation");
            var first = await dr.GetCurrentDataAsync(l);
            Assert.IsNotNull(first);
            first.Pulled = DateTimeOffset.Now.AddMinutes(-10);
            current = refreshData;
            var second = await dr.GetCurrentDataAsync(l);
            Assert.IsNotNull(second);
            Assert.IsNull(second.Humidity);
            Assert.IsNotNull(second.CurrentTemperature);
            Assert.AreEqual(75, second.CurrentTemperature!.To(TemperatureEnum.Fahrenheit));
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldOverlaySunData()
        {
            var currentService = SetupCurrentQueryService("Current");
            var sunService = SetupSunDataService("Sun");
            SetupDefaultAlert();
            SetupDefaultHistory();
            var wc = GenerateWeatherConditions(latitude:45);
            var currentData = GenerateCurrentData(weatherConditions: wc);
            currentService.Setup(s=>s.GetCurrentDataAsync(It.IsAny<Location>())).ReturnsAsync(currentData);

            var moonData = new MoonData(DateTimeOffset.Now)
            {
                Latitude = 45,
                Moonrise = DateTimeOffset.Now.Date.AddHours(1),
                Moonset = DateTimeOffset.Now.Date.AddHours(13),
                MoonDeclination = new MoonProperty(10, DateTimeOffset.Now)
            };
            Assert.IsNotNull(moonData.MoonAngleAt(DateTime.Now), "Moon Angle could not be inferred! Some data is missing, or the method is broken.");
            var solarNoon = DateTimeOffset.Now.Date.AddHours(12);
            var sunLine = new SunLine(DateTimeOffset.Now) { SolarNoon = solarNoon, MoonData = moonData, For = DateTime.Today };
            var sunData = new SunData(DateTimeOffset.Now, sunLine);
            sunService.Setup(s=>s.GetSunDataAsync(It.IsAny<Location>(),It.IsAny<DateTimeOffset>(),It.IsAny<DateTimeOffset>())).ReturnsAsync(sunData);

            Config.CurrentData = GenerateAllStarConfig("Current");
            Config.SunData = GenerateAllStarConfig("Sun");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");
            var result = await dr.GetCurrentDataAsync(l);
            Assert.IsNotNull(result);
            var expectedAngle = GetExpectedSunAngle(wc, solarNoon);
            Assert.IsNotNull(result.WeatherConditions?.SunAngle, "Sun Angle is not provided in the result!");
            Assert.AreEqual(expectedAngle, result.WeatherConditions.SunAngle.Value, 0.01);
            var moonDataAngle = moonData.MoonAngleAt(DateTime.Now);
            Assert.IsNotNull(moonDataAngle);
            Assert.IsNotNull(result.WeatherConditions?.MoonAngle, $"Moon Angle wasn't merged when it should have been. {(result.WeatherConditions == null ? "WeatherConditions is null" : "")} / {(result.WeatherConditions?.MoonAngle == null ? "MoonAngle is null" : "")}");
            Assert.AreEqual(moonDataAngle.Value, result.WeatherConditions.MoonAngle.Value, 0.01);
        }

        private static float GetExpectedSunAngle(WeatherConditions wc, DateTimeOffset solarNoon)
        {
            var dayOfYear = (double)wc.Time.DayOfYear;
            var declinationDegrees = -23.4 * Math.Cos((360.0 / 365 * (dayOfYear + 10)).ToRadians());
            var declinationRadians = declinationDegrees.ToRadians();
            var phiRadians = wc.Latitude!.Value.ToRadians();
            var timeDifference = (DateTime.Now - solarNoon).TotalSeconds / 3600;
            var hourAngleDegrees = 15 * timeDifference;
            var hourAngleRadians = hourAngleDegrees.ToRadians();
            var sinElevation = (Math.Sin(phiRadians) * Math.Sin(declinationRadians)) +
                               (Math.Cos(phiRadians) * Math.Cos(declinationRadians) * Math.Cos(hourAngleRadians));
            return (float)Math.Asin(sinElevation).ToDegrees();
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldOverlayAlertData()
        {
            var currentService = SetupCurrentQueryService("Current");
            var alertService = SetupAlertQueryService("Alert");
            SetupDefaultSun();
            SetupDefaultHistory();
            currentService.Setup(s=>s.GetCurrentDataAsync(It.IsAny<Location>())).ReturnsAsync(GenerateCurrentData());
            var alert = new Alert(DateTimeOffset.Now)
            {
                Headline = "Tornado warning",
                Severity = AlertSeverityEnum.Severe,
                Urgency = AlertUrgencyEnum.Immediate
            };
            var alertData = new AlertData(DateTimeOffset.Now, alert);
            alertService.Setup(a=>a.GetAlertDataAsync(It.IsAny<Location>())).ReturnsAsync(alertData);
            Config.CurrentData = GenerateAllStarConfig("Current");
            Config.AlertData = GenerateAllStarConfig("Alert");
            var dr = GetDataRetriever();
            var l = new Location("TestLocation");
            var result = await dr.GetCurrentDataAsync(l);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.WeatherConditions!.IsTornado!.Value);
            Assert.IsTrue(result.WeatherConditions!.IsWarning!.Value);
            Assert.IsFalse(result.WeatherConditions!.IsHurricane!.Value);
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldAppendHistoryLineOnRetrieval()
        {
            var currentService = SetupCurrentQueryService("Current");
            var historyService = SetupHistoryQueryService("History");
            SetupDefaultSun();
            SetupDefaultAlert();
            var historyData = new HistoryData(DateTimeOffset.Now);
            historyService.Setup(h=>h.GetHistoryDataAsync(It.IsAny<Location>(),It.IsAny<DateTimeOffset>(),It.IsAny<DateTimeOffset>())).ReturnsAsync(historyData);
            currentService.Setup(s=>s.GetCurrentDataAsync(It.IsAny<Location>())).ReturnsAsync(GenerateCurrentData());
            Config.CurrentData = GenerateAllStarConfig("Current");
            Config.HistoryData = GenerateAllStarConfig("History");
            var dr = GetDataRetriever();
            var l = new Location("TestLocation");
            await dr.GetCurrentDataAsync(l);
            var stored = await dr.GetHistoryDataAsync(l);
            Assert.IsNotNull(stored);
            Assert.AreEqual(1, stored!.Lines.Count());
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldThrowIfServiceThrowsException()
        {
            var service = SetupCurrentQueryService("Current");
            service.Setup(s=>s.GetCurrentDataAsync(It.IsAny<Location>())).ThrowsAsync(new InvalidOperationException("fail"));
            SetupDefaults();
            Config.CurrentData = GenerateAllStarConfig("Current");
            var dr = GetDataRetriever();
            var l = new Location("TestLocation");
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => dr.GetCurrentDataAsync(l));
            Assert.AreEqual(0, CurrentDataUpdated.Count);
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldWeightHumidityCorrectly()
        {
            SetupDefaults();
            await TestCurrentFloatWeighting(nameof(DataLine.Humidity), (c,v)=>c.Humidity=v, c=>c.Humidity);
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldWeightCurrentTemperatureCorrectly()
        {
            SetupDefaults();
            await TestCurrentTemperatureWeighting(nameof(DataLine.CurrentTemperature), (c,t)=>c.CurrentTemperature=t, c=>c.CurrentTemperature);
        }
    }
}
