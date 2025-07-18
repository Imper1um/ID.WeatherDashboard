using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Codes;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.APITests.Services
{
    [TestClass]
    public class DataRetrieverServiceTests_Forecast : DataRetrieverServiceTests
    {
        private Mock<IForecastQueryService> SetupForecastQueryService(string name)
        {
            var s = new Mock<IForecastQueryService>();
            s.SetupGet(m => m.ServiceName).Returns(name);
            ForecastQueryServiceMocks.Add(s);
            return s;
        }

        private ForecastLine GenerateForecastLine(
            DateTimeOffset? pulled = null,
            string[]? sources = null,
            DateTimeOffset? observed = null,
            float? rainChance = null,
            float? snowChance = null,
            WeatherConditions? weatherConditions = null)
        {
            return new ForecastLine(pulled ?? DateTimeOffset.Now, sources ?? Array.Empty<string>())
            {
                Observed = observed ?? DateTimeOffset.Now,
                RainChance = rainChance,
                SnowChance = snowChance,
                WeatherConditions = weatherConditions ?? GenerateWeatherConditions()
            };
        }

        private ForecastDay GenerateForecastDay(
            DateTimeOffset? pulled = null,
            string[]? sources = null,
            DateTimeOffset? observed = null,
            Temperature? high = null,
            Temperature? low = null,
            float? rainChance = null,
            float? snowChance = null,
            IEnumerable<ForecastLine>? lines = null)
        {
            var day = new ForecastDay(pulled ?? DateTimeOffset.Now, sources ?? Array.Empty<string>())
            {
                Observed = observed ?? DateTimeOffset.Now.DayOf(),
                RainChance = rainChance,
                SnowChance = snowChance,
                Daytime = new DaytimeData { High = high },
                Nighttime = new NighttimeData { Low = low },
                WeatherConditions = GenerateWeatherConditions()
            };
            foreach (var line in lines ?? new[] { GenerateForecastLine(observed: (observed ?? DateTimeOffset.Now.DayOf()).AddHours(12)) })
            {
                day.AddLine(line);
            }
            return day;
        }

        private ForecastData GenerateForecastData(
            DateTimeOffset? pulled = null,
            IEnumerable<ForecastDay>? days = null)
        {
            return new ForecastData(pulled ?? DateTimeOffset.Now, days ?? new[] { GenerateForecastDay() });
        }

        private async Task TestForecastTemperatureWeighting(string elementName, Action<ForecastDay, Temperature> setValue, Func<ForecastDay, Temperature?> getValue)
        {
            var weight1 = TestHelpers.RandomIntBetween(100, 500);
            var weight2 = TestHelpers.RandomIntBetween(100, 500);

            var service1Name = TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters);
            var service2Name = TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters);

            var service1 = SetupForecastQueryService(service1Name);
            var service2 = SetupForecastQueryService(service2Name);

            var observed = DateTimeOffset.Now.DayOf();

            var value1 = TestHelpers.RandomFloatBetween(60, 100);
            var value2 = TestHelpers.RandomFloatBetween(60, 100);

            var day1 = GenerateForecastDay(observed: observed);
            var day2 = GenerateForecastDay(observed: observed);

            setValue(day1, new Temperature(value1));
            setValue(day2, new Temperature(value2));

            service1.Setup(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new ForecastData(DateTimeOffset.Now, new[] { day1 }));
            service2.Setup(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new ForecastData(DateTimeOffset.Now, new[] { day2 }));

            Config.ForecastData = new DataConfig()
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

            var result = await dr.GetForecastDataAsync(l);

            Assert.IsNotNull(result);
            var day = result.Days.First();
            var temp = getValue(day);
            Assert.IsNotNull(temp);

            var expected = ((value1 * weight1) + (value2 * weight2)) / (float)(weight1 + weight2);
            Assert.AreEqual(expected, temp!.To(TemperatureEnum.Fahrenheit)!.Value, 0.01, $"Weighted average for {elementName} incorrect.");
        }

        private async Task TestForecastLineFloatWeighting(string elementName, Action<ForecastLine, float> setValue, Func<ForecastLine, float?> getValue)
        {
            var weight1 = TestHelpers.RandomIntBetween(100, 500);
            var weight2 = TestHelpers.RandomIntBetween(100, 500);

            var service1Name = TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters);
            var service2Name = TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters);

            var service1 = SetupForecastQueryService(service1Name);
            var service2 = SetupForecastQueryService(service2Name);

            var observed = DateTimeOffset.Now.HourOf();

            var value1 = TestHelpers.RandomFloatBetween(0, 1);
            var value2 = TestHelpers.RandomFloatBetween(0, 1);

            var line1 = GenerateForecastLine(observed: observed);
            var line2 = GenerateForecastLine(observed: observed);

            setValue(line1, value1);
            setValue(line2, value2);

            var day1 = GenerateForecastDay(observed: observed.DayOf(), lines: new[] { line1 });
            var day2 = GenerateForecastDay(observed: observed.DayOf(), lines: new[] { line2 });

            service1.Setup(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new ForecastData(DateTimeOffset.Now, new[] { day1 }));
            service2.Setup(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new ForecastData(DateTimeOffset.Now, new[] { day2 }));

            Config.ForecastData = new DataConfig
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

            var result = await dr.GetForecastDataAsync(l);

            Assert.IsNotNull(result);
            var line = result.Days.SelectMany(d => d.Lines).First(lin => lin.Observed?.HourOf() == observed);
            var val = getValue(line);
            Assert.IsNotNull(val);
            var expected = ((value1 * weight1) + (value2 * weight2)) / (float)(weight1 + weight2);
            Assert.AreEqual(expected, val!.Value, 0.01, $"Weighted average for {elementName} incorrect.");
        }

        private async Task TestForecastLineDateWeighting(string elementName, DateTimeOffset date1, DateTimeOffset date2, int weight1, int weight2)
        {
            var service1Name = TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters);
            var service2Name = TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters);

            var service1 = SetupForecastQueryService(service1Name);
            var service2 = SetupForecastQueryService(service2Name);

            var line1 = GenerateForecastLine(observed: date1);
            var line2 = GenerateForecastLine(observed: date2);

            var day1 = GenerateForecastDay(observed: date1.DayOf(), lines: new[] { line1 });
            var day2 = GenerateForecastDay(observed: date2.DayOf(), lines: new[] { line2 });

            service1.Setup(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new ForecastData(DateTimeOffset.Now, new[] { day1 }));
            service2.Setup(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new ForecastData(DateTimeOffset.Now, new[] { day2 }));

            Config.ForecastData = new DataConfig
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

            var result = await dr.GetForecastDataAsync(l);

            Assert.IsNotNull(result);
            var line = result.Days.SelectMany(d => d.Lines).First();
            Assert.IsNotNull(line.Observed);

            var minDate = date1 < date2 ? date1 : date2;
            var maxDate = date1 > date2 ? date1 : date2;

            double ticks1 = 0;
            double ticks2 = (maxDate.UtcTicks - minDate.UtcTicks);
            double expectedWeighted = ((ticks1 * (date1 < date2 ? weight1 : weight2)) + (ticks2 * (date1 < date2 ? weight2 : weight1))) / (double)(weight1 + weight2);
            var expected = minDate.AddTicks((long)expectedWeighted);

            Assert.AreEqual(expected.UtcTicks, line.Observed.Value.UtcTicks, 100000, "Weighted average for observed incorrect.");
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldErrorIfNoServicesConfigured()
        {
            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            Config.ForecastData = GenerateAllStarConfig("ForecastService");

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => dr.GetForecastDataAsync(l));
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldErrorIfNoConfigProvided()
        {
            var service = SetupForecastQueryService("ForecastService");
            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => dr.GetForecastDataAsync(l));

            service.Verify(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Never());
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldErrorIfNoDataReturnedFromServices()
        {
            var service = SetupForecastQueryService("ForecastService");
            service.Setup(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync((ForecastData?)null);

            Config.ForecastData = GenerateAllStarConfig("ForecastService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => dr.GetForecastDataAsync(l));

            service.Verify(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Once());
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldReturnForecastDataWithDaysWhenValidDataReturned()
        {
            var service = SetupForecastQueryService("ForecastService");

            var day = GenerateForecastDay();
            var data = new ForecastData(DateTimeOffset.Now, new[] { day });

            service.Setup(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(data);

            Config.ForecastData = GenerateAllStarConfig("ForecastService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetForecastDataAsync(l);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Days.Any());
            Assert.AreEqual(day.Observed, result.Days.First().Observed);
            Assert.AreEqual(1, ForecastDataUpdated.Count);
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldReturnCachedDataIfNotExpired()
        {
            var service = SetupForecastQueryService("ForecastService");

            var data = GenerateForecastData();
            service.Setup(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(data);

            Config.ForecastData = GenerateAllStarConfig("ForecastService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var first = await dr.GetForecastDataAsync(l);
            var second = await dr.GetForecastDataAsync(l);

            Assert.AreSame(first, second);
            service.Verify(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Once());
            Assert.AreEqual(1, ForecastDataUpdated.Count);
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldRefreshDataIfCacheExpired()
        {
            var service = SetupForecastQueryService("ForecastService");

            var initial = GenerateForecastData();
            var refreshed = GenerateForecastData(days: new[] { GenerateForecastDay(observed: DateTimeOffset.Now.DayOf().AddDays(1)) });

            var toReturn = initial;
            service.Setup(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(() => toReturn);

            Config.ForecastData = GenerateAllStarConfig("ForecastService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var first = await dr.GetForecastDataAsync(l);
            first.Pulled = DateTimeOffset.Now.AddMinutes(-10);
            first.ReplaceLines(Array.Empty<ForecastDay>());

            toReturn = refreshed;

            var second = await dr.GetForecastDataAsync(l);

            Assert.IsTrue(second.Days.Any(d => d.Observed == refreshed.Days.First().Observed));
            service.Verify(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Exactly(2));
            Assert.AreEqual(2, ForecastDataUpdated.Count);
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldReplaceDataIfOverlayExistingDataIsFalse()
        {
            var service = SetupForecastQueryService("ForecastService");

            var initial = GenerateForecastData();
            var initialObserved = initial.Days.First().Observed;
            var newDay = GenerateForecastDay(observed: DateTimeOffset.Now.DayOf().AddDays(1));
            var refreshed = new ForecastData(DateTimeOffset.Now, new[] { newDay });

            var current = initial;
            service.Setup(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(() => current);

            Config.ForecastData = GenerateAllStarConfig("ForecastService");
            Config.ForecastData.OverlayExistingData = false;

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var first = await dr.GetForecastDataAsync(l);
            first.Pulled = DateTimeOffset.Now.AddMinutes(-10);
            first.ReplaceLines(Array.Empty<ForecastDay>());
            current = refreshed;

            var second = await dr.GetForecastDataAsync(l);

            Assert.IsTrue(second.Days.Any(d => d.Observed == newDay.Observed));
            service.Verify(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Exactly(2));
            Assert.AreEqual(2, ForecastDataUpdated.Count);
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldMergeDataIfOverlayExistingDataIsTrue()
        {
            var service = SetupForecastQueryService("ForecastService");

            var initial = GenerateForecastData();
            var newDay = GenerateForecastDay(observed: DateTimeOffset.Now.DayOf().AddDays(1));
            var refreshed = new ForecastData(DateTimeOffset.Now, new[] { newDay });

            var current = initial;
            service.Setup(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(() => current);

            Config.ForecastData = GenerateAllStarConfig("ForecastService");
            Config.ForecastData.OverlayExistingData = true;

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var first = await dr.GetForecastDataAsync(l);
            first.Pulled = DateTimeOffset.Now.AddMinutes(-10);
            first.ReplaceLines(Array.Empty<ForecastDay>());
            current = refreshed;

            var second = await dr.GetForecastDataAsync(l);

            var days = second.Days.ToList();
            Assert.IsTrue(days.Any(d => d.Observed == initial.Days.First().Observed));
            Assert.IsTrue(days.Any(d => d.Observed == newDay.Observed));
            service.Verify(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Exactly(2));
            Assert.AreEqual(2, ForecastDataUpdated.Count);
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldMergeDaysWithSameDateCorrectly()
        {
            var service1 = SetupForecastQueryService("Forecast1");
            var service2 = SetupForecastQueryService("Forecast2");

            var observed = DateTimeOffset.Now.DayOf();
            var day1 = GenerateForecastDay(observed: observed);
            var day2 = GenerateForecastDay(observed: observed);

            service1.Setup(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new ForecastData(DateTimeOffset.Now, new[] { day1 }));
            service2.Setup(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new ForecastData(DateTimeOffset.Now, new[] { day2 }));

            Config.ForecastData = GenerateAllStarConfig("Forecast1", "Forecast2");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetForecastDataAsync(l);

            Assert.AreEqual(1, result.Days.Count(d => d.Observed == observed));
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldWeightForecastHighTemperatureCorrectly()
        {
            await TestForecastTemperatureWeighting($"{nameof(ForecastDay.Daytime)}.{nameof(DaytimeData.High)}", (d, t) => d.Daytime!.High = t, d => d.Daytime!.High);
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldWeightForecastLowTemperatureCorrectly()
        {
            await TestForecastTemperatureWeighting($"{nameof(ForecastDay.Nighttime)}.{nameof(NighttimeData.Low)}", (d, t) => d.Nighttime!.Low = t, d => d.Nighttime!.Low);
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldWeightRainChanceCorrectly()
        {
            await TestForecastLineFloatWeighting(nameof(ForecastLine.RainChance), (l, f) => l.RainChance = f, l => l.RainChance);
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldWeightSnowChanceCorrectly()
        {
            await TestForecastLineFloatWeighting(nameof(ForecastLine.SnowChance), (l, f) => l.SnowChance = f, l => l.SnowChance);
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldWeightObservedDateCorrectly()
        {
            var date1 = DateTimeOffset.Now.HourOf();
            var date2 = date1.AddMinutes(30);
            await TestForecastLineDateWeighting(nameof(DataLine.Observed), date1, date2, 100, 200);
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldRaiseForecastDataUpdatedEventOnSuccessfulRetrieval()
        {
            var service = SetupForecastQueryService("ForecastService");
            var day = GenerateForecastDay();
            service.Setup(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new ForecastData(DateTimeOffset.Now, new[] { day }));

            Config.ForecastData = GenerateAllStarConfig("ForecastService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetForecastDataAsync(l);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, ForecastDataUpdated.Count);
            Assert.AreEqual(l, ForecastDataUpdated.First().Location);
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldThrowIfServiceThrowsException()
        {
            var service = SetupForecastQueryService("ForecastService");
            service.Setup(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ThrowsAsync(new InvalidOperationException("fail"));

            Config.ForecastData = GenerateAllStarConfig("ForecastService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => dr.GetForecastDataAsync(l));
            Assert.AreEqual(0, ForecastDataUpdated.Count);
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldReturnEmptyForecastDataWhenServicesReturnEmptyDays()
        {
            var service = SetupForecastQueryService("ForecastService");
            service.Setup(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new ForecastData(DateTimeOffset.Now));

            Config.ForecastData = GenerateAllStarConfig("ForecastService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetForecastDataAsync(l);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Days.Any());
            Assert.AreEqual(1, ForecastDataUpdated.Count);
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldNotCallServiceAgainIfDataIsValidAndCacheExists()
        {
            var service = SetupForecastQueryService("ForecastService");
            var data = GenerateForecastData();
            service.Setup(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(data);

            Config.ForecastData = GenerateAllStarConfig("ForecastService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var first = await dr.GetForecastDataAsync(l);
            var second = await dr.GetForecastDataAsync(l);

            Assert.AreSame(first, second);
            service.Verify(s => s.GetForecastDataAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()), Times.Once());
        }

    }
}

