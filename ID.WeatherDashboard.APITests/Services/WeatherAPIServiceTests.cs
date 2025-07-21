using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.API.Codes;
using ID.WeatherDashboard.WeatherAPI.Data;
using ID.WeatherDashboard.WeatherAPI.Services;
using Moq;

namespace ID.WeatherDashboard.APITests.Services
{
    [TestClass]
    public class WeatherAPIServiceTests
    {
        private Mock<IJsonQueryService> jsonQuery = null!;
        private WeatherAPIService service = null!;

        [TestInitialize]
        public void Init()
        {
            jsonQuery = new Mock<IJsonQueryService>();
            service = new WeatherAPIService(jsonQuery.Object);
        }

        private ServiceConfig SetConfig(string? apiKey = null, string? name = null, int maximumCallsPerHour = 100, int maximumCallsPerDay = 100)
        {
            var config = new ServiceConfig()
            {
                ApiKey = apiKey ?? TestHelpers.RandomString(8, TestHelpers.UppercaseLetters),
                Name = name ?? TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters),
                MaxCallsPerDay = maximumCallsPerDay,
                MaxCallsPerHour = maximumCallsPerHour,
                Assembly = "",
                Type = ""
            };
            service.SetServiceConfig(config);
            return config;
        }

        private static WeatherApiLocation GenerateLocation()
        {
            return new WeatherApiLocation
            {
                Latitude = TestHelpers.RandomDoubleBetween(-90, 90),
                Longitude = TestHelpers.RandomDoubleBetween(-180, 180),
                TimezoneId = "UTC"
            };
        }

        private static WeatherApiCurrentAPI GenerateCurrentApi()
        {
            var epoch = TestHelpers.RandomDateTimeOffsetBetween(DateTimeOffset.Now.AddHours(-1), DateTimeOffset.Now).ToUnixTimeSeconds();
            return new WeatherApiCurrentAPI
            {
                Pulled = DateTimeOffset.Now,
                Location = GenerateLocation(),
                Current = new WeatherApiCurrent
                {
                    LastUpdatedEpoch = epoch,
                    TempFahrenheit = TestHelpers.RandomFloatBetween(-30, 110),
                    FeelsLikeFahrenheit = TestHelpers.RandomFloatBetween(-30, 110),
                    HeatIndexFahrenheit = TestHelpers.RandomFloatBetween(-30, 120),
                    DewpointFahrenheit = TestHelpers.RandomFloatBetween(0, 70),
                    Humidity = TestHelpers.RandomFloatBetween(0, 100),
                    UvIndex = TestHelpers.RandomFloatBetween(0, 11),
                    PressureInches = TestHelpers.RandomFloatBetween(25, 32),
                    WindDegree = TestHelpers.RandomIntBetween(0, 360),
                    PrecipitationInches = TestHelpers.RandomFloatBetween(0, 2),
                    GustMilesPerHour = TestHelpers.RandomFloatBetween(0, 60),
                    WindMph = TestHelpers.RandomFloatBetween(0, 40),
                    VisibilityMiles = TestHelpers.RandomFloatBetween(0, 10),
                    CloudCoverPercentage = TestHelpers.RandomFloatBetween(0, 100),
                    Condition = new WeatherApiCondition { Code = 1183, Text = "Light rain" }
                }
            };
        }

        private static WeatherApiForecastAPI GenerateForecastApi(DateTimeOffset date)
        {
            var hourEpoch = date.ToUnixTimeSeconds();
            return new WeatherApiForecastAPI
            {
                Pulled = DateTimeOffset.Now,
                Forecast = new WeatherApiForecast
                {
                    ForecastDays =
                    [
                        new WeatherApiForecastDay
                        {
                            DateEpoch = date.ToUnixTimeSeconds(),
                            Day = new WeatherApiDay
                            {
                                MaximumTemperatureFahrenheit = TestHelpers.RandomFloatBetween(60, 100),
                                MinimumTemperatureFahrenheit = TestHelpers.RandomFloatBetween(30, 60),
                                Condition = new WeatherApiCondition { Text = "Sunny", Code = 1000 }
                            },
                            Hours = [ new WeatherApiHour
                                {
                                    LastUpdatedEpoch = hourEpoch,
                                    TempFahrenheit = TestHelpers.RandomFloatBetween(60, 100),
                                    ChanceOfRain = TestHelpers.RandomFloatBetween(0, 100),
                                    ChanceOfSnow = TestHelpers.RandomFloatBetween(0, 100),
                                    Condition = new WeatherApiCondition { Code = 1000 }
                                }
                            ]
                        }
                    ]
                }
            };
        }

        private static WeatherApiHistoryAPI GenerateHistoryApi(DateTimeOffset date)
        {
            var epoch = date.ToUnixTimeSeconds();
            return new WeatherApiHistoryAPI
            {
                Pulled = DateTimeOffset.Now,
                Forecast = new WeatherApiForecast
                {
                    ForecastDays = [ new WeatherApiForecastDay
                        {
                            DateEpoch = epoch,
                            Hours = [ new WeatherApiHour
                                {
                                    LastUpdatedEpoch = epoch,
                                    TempFahrenheit = TestHelpers.RandomFloatBetween(50, 100)
                                }
                            ]
                        }
                    ]
                }
            };
        }

        private static WeatherApiAstronomyAPI GenerateAstronomyApi(DateTime date, double lat, double lon)
        {
            return new WeatherApiAstronomyAPI(date)
            {
                Pulled = DateTimeOffset.Now,
                Location = new WeatherApiLocation { Latitude = lat, Longitude = lon, TimezoneId = "UTC" },
                Astronomy = new WeatherApiAstronomy
                {
                    Astro = new WeatherApiAstro
                    {
                        Sunrise = "06:00 AM",
                        Sunset = "06:00 PM",
                        Moonrise = "06:30 AM",
                        Moonset = "06:30 PM",
                        MoonPhase = "Full Moon"
                    }
                }
            };
        }

        private static WeatherApiAlertAPI GenerateAlertApi()
        {
            return new WeatherApiAlertAPI
            {
                Pulled = DateTimeOffset.Now,
                Alerts = new WeatherApiAlerts
                {
                    Alerts = [ new WeatherApiAlert
                        {
                            Headline = TestHelpers.RandomString(10, TestHelpers.UppercaseLetters),
                            MessageType = "Alert",
                            Severity = "Severe",
                            Urgency = "Immediate",
                            Certainty = "Observed",
                            Category = "Met",
                            Event = "Storm",
                            Note = "Test",
                            Description = "Testing",
                            Instruction = "Stay inside"
                        }
                    ]
                }
            };
        }
        private void CheckResult(float? apiResult, float? expectedResult, float delta, string name)
        {
            if (expectedResult == null)
            {
                Assert.IsNull(apiResult, $"API Result is not null for {name} and it should be.");
                return;
            }
            Assert.IsNotNull(apiResult, $"API Result for {name} is null, and should not be.");
            Assert.AreEqual(apiResult.Value, expectedResult.Value, delta, $"{name} has too much separation.");
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldRequestCorrectUrlAndMapData()
        {
            var config = SetConfig();

            var locationName = TestHelpers.RandomString(6, TestHelpers.UppercaseLetters);
            var location = new Location(locationName);

            var apiResult = GenerateCurrentApi();
            string? requestedUrl = null;
            jsonQuery.Setup(j => j.QueryAsync<WeatherApiCurrentAPI>(It.IsAny<string>(), It.IsAny<Tuple<string, string>[]>() ))
                .Callback<string, Tuple<string,string>[]>((u, _) => requestedUrl = u)
                .ReturnsAsync(apiResult);

            var result = await service.GetCurrentDataAsync(location);

            var expectedUrl = $"{WeatherAPIService._currentUrl}?key={config.ApiKey}&q={location}";
            Assert.AreEqual(expectedUrl, requestedUrl, $"URL mismatch for location {location}. Expected {expectedUrl} but got {requestedUrl}.");
            Assert.IsNotNull(result, "Service returned null CurrentData while a valid API response was supplied.");
            Assert.IsNotNull(apiResult.Current, $"{nameof(WeatherApiCurrent)} is null and it shouldn't be.");
            Assert.IsNotNull(apiResult.Current.LastUpdatedEpoch, $"{nameof(WeatherApiCurrent.LastUpdatedEpoch)} is null and it shouldn't be.");
            Assert.AreEqual(DateTimeOffset.FromUnixTimeSeconds(apiResult.Current.LastUpdatedEpoch.Value), result.Observed, "Observed timestamp was not converted correctly.");
            CheckResult(apiResult.Current.TempFahrenheit, result.CurrentTemperature?.To(TemperatureEnum.Fahrenheit), 0.001f, nameof(WeatherApiCurrent.TempFahrenheit));
            CheckResult(apiResult.Current.FeelsLikeFahrenheit, result.FeelsLike?.To(TemperatureEnum.Fahrenheit), 0.001f, nameof(WeatherApiCurrent.FeelsLikeFahrenheit));
            Assert.AreEqual(apiResult.Current.UvIndex, result.UVIndex, "UV index mismatch.");
            CheckResult(apiResult.Current.PressureInches, result.Pressure?.To(PressureEnum.InchesOfMercury), 0.001f, nameof(WeatherApiCurrent.PressureInches));
            CheckResult(apiResult.Current.WindMph, result.WeatherConditions?.WindSpeed?.To(WindSpeedEnum.MilesPerHour), 0.001f, nameof(WeatherConditions.WindSpeed));
        }

        [TestMethod]
        public async Task GetCurrentDataAsync_ShouldReturnNullWhenServiceReturnsNull()
        {
            service.SetServiceConfig(new ServiceConfig { ApiKey = "key", Name = "WA",
                Assembly = "",
                Type = ""
            });
            jsonQuery.Setup(j => j.QueryAsync<WeatherApiCurrentAPI>(It.IsAny<string>(), It.IsAny<Tuple<string,string>[]>() ))
                .ReturnsAsync((WeatherApiCurrentAPI?)null);

            var result = await service.GetCurrentDataAsync(new Location("nowhere"));

            Assert.IsNull(result, "Expected null CurrentData when query service returned null.");
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldRequestCorrectUrlAndMapData()
        {
            var config = SetConfig();
            var location = new Location("ForecastTown");
            var start = new DateTimeOffset(DateTimeOffset.Now.AddDays(1).Date, TimeSpan.Zero);
            var end = start.AddDays(1);

            var apiResult = GenerateForecastApi(start);
            string? url = null;
            jsonQuery.Setup(j => j.QueryAsync<WeatherApiForecastAPI>(It.IsAny<string>(), It.IsAny<Tuple<string,string>[]>() ))
                .Callback<string, Tuple<string,string>[]>((u, _) => url = u)
                .ReturnsAsync(apiResult);

            var result = await service.GetForecastDataAsync(location, start, end);
            jsonQuery.Verify(j => j.QueryAsync<WeatherApiForecastAPI>(It.IsAny<string>(), It.IsAny<Tuple<string, string>[]>()), Times.AtLeastOnce);

            var expectedUrl = $"{WeatherAPIService._forecastUrl}?key={config.ApiKey}&q={location}&dt={start.ToUnixTimeSeconds()}&days=2";
            Assert.AreEqual(expectedUrl, url, $"Forecast URL mismatch. Expected {expectedUrl} but got {url}.");
            Assert.IsNotNull(result, "ForecastData should not be null when API provides data.");
            var day = result!.Days.First();
            Assert.IsNotNull(apiResult.Forecast?.ForecastDays, $"{nameof(WeatherApiForecast.ForecastDays)} is null and it shouldn't be.");
            Assert.AreEqual(1, apiResult.Forecast.ForecastDays.Count, $"{nameof(WeatherApiForecast.ForecastDays)} is empty and it shouldn't be.");
            var forecastDay = apiResult.Forecast.ForecastDays.First();
            CheckResult(forecastDay.Day?.MaximumTemperatureFahrenheit, day.Daytime!.High!.To(TemperatureEnum.Fahrenheit), 0.001f, nameof(WeatherApiDay.MaximumTemperatureFahrenheit));
            var line = day.Lines.First();
            CheckResult(apiResult.Forecast.ForecastDays[0].Hours![0].TempFahrenheit, line.CurrentTemperature!.To(TemperatureEnum.Fahrenheit), 0.001f, "Hourly temperature mapping incorrect.");
        }

        [TestMethod]
        public async Task GetForecastDataAsync_ShouldThrowWhenRangeExceedsLimit()
        {
            var config = SetConfig();
            var from = new DateTimeOffset(DateTimeOffset.Now.Date, TimeSpan.Zero);
            var to = from.AddDays(15);
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.GetForecastDataAsync(new Location("a"), from, to));
        }

        [TestMethod]
        public async Task GetHistoryDataAsync_ShouldRequestCorrectUrlAndMapData()
        {
            var config = SetConfig();
            var location = new Location("HistTown");
            var start = new DateTimeOffset(DateTimeOffset.Now.AddDays(-1).Date, TimeSpan.Zero);
            var end = new DateTimeOffset(DateTimeOffset.Now.Date, TimeSpan.Zero);

            var apiResult = GenerateHistoryApi(start);
            string? reqUrl = null;
            jsonQuery.Setup(j => j.QueryAsync<WeatherApiHistoryAPI>(It.IsAny<string>(), It.IsAny<Tuple<string,string>[]>() ))
                .Callback<string, Tuple<string,string>[]>((u, _) => reqUrl = u)
                .ReturnsAsync(apiResult);

            var result = await service.GetHistoryDataAsync(location, start, end);
            jsonQuery.Verify(j => j.QueryAsync<WeatherApiHistoryAPI>(It.IsAny<string>(), It.IsAny<Tuple<string, string>[]>()), Times.AtLeastOnce);

            var expectedUrl = $"{WeatherAPIService._historyUrl}?key={config.ApiKey}&q={location}&dt={start.ToUnixTimeSeconds()}&end_dt={end.ToUnixTimeSeconds()}";
            Assert.AreEqual(expectedUrl, reqUrl, $"History URL mismatch. Expected {expectedUrl} but got {reqUrl}.");
            Assert.IsNotNull(result, "HistoryData should not be null for valid API response.");
            Assert.IsNotNull(result.Lines, $"{nameof(HistoryData.Lines)} is null and it shouldn't be.");
            Assert.IsTrue(result.Lines.Any(), $"{nameof(HistoryData.Lines)} is empty and it shouldn't be.");
            var l = result.Lines.First();
            CheckResult(apiResult.Forecast!.ForecastDays![0].Hours![0].TempFahrenheit, l.CurrentTemperature?.To(TemperatureEnum.Fahrenheit), 0.001f, nameof(HistoryLine.CurrentTemperature));
        }

        [TestMethod]
        public async Task GetHistoryDataAsync_ShouldThrowWhenRangeExceedsLimit()
        {
            SetConfig();
            var from = new DateTimeOffset(DateTimeOffset.Now.AddDays(-31).Date, TimeSpan.Zero);
            var to = new DateTimeOffset(DateTimeOffset.Now.Date, TimeSpan.Zero);
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.GetHistoryDataAsync(new Location("x"), from, to));
        }

        [TestMethod]
        public async Task GetSunDataAsync_ShouldAggregateDailyResults()
        {
            var config = SetConfig();
            var location = new Location("SunCity") { Latitude = 0, Longitude = 0 };
            var start = new DateTimeOffset(new DateTime(2024, 1, 1), TimeSpan.Zero);
            var end = start.AddDays(1);

            var api1 = GenerateAstronomyApi(start.Date, 0, 0);
            var api2 = GenerateAstronomyApi(end.Date, 0, 0);
            jsonQuery.SetupSequence(j => j.QueryAsync<WeatherApiAstronomyAPI>(It.IsAny<string>(), It.IsAny<Tuple<string,string>[]>() ))
                .ReturnsAsync(api1)
                .ReturnsAsync(api2);

            var data = await service.GetSunDataAsync(location, start, end);

            var expectedUrl = $"{WeatherAPIService._astronomyUrl}?key={config.ApiKey}&q={location}&dt={start:yyyy-MM-dd}";
            jsonQuery.Verify(j => j.QueryAsync<WeatherApiAstronomyAPI>(expectedUrl, It.IsAny<Tuple<string,string>[]>()), Times.Once());
            Assert.IsNotNull(data, "SunData should not be null.");
            Assert.AreEqual(2, data!.Lines.Count(), $"Expected 2 sun lines for range {start:d} to {end:d}.");
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldRequestCorrectUrlAndMapData()
        {
            var config = SetConfig();
            var location = new Location("AlertTown");

            var apiResult = GenerateAlertApi();
            string? url = null;
            jsonQuery.Setup(j => j.QueryAsync<WeatherApiAlertAPI>(It.IsAny<string>(), It.IsAny<Tuple<string,string>[]>() ))
                .Callback<string, Tuple<string,string>[]>((u, _) => url = u)
                .ReturnsAsync(apiResult);

            var result = await service.GetAlertDataAsync(location);
            jsonQuery.Verify(j => j.QueryAsync<WeatherApiAlertAPI>(It.IsAny<string>(), It.IsAny<Tuple<string, string>[]>()));

            var expectedUrl = $"{WeatherAPIService._alertUrl}?key={config.ApiKey}&q={location}";
            Assert.AreEqual(expectedUrl, url, $"Alert URL mismatch. Expected {expectedUrl} but got {url}.");
            Assert.IsNotNull(result, "AlertData should not be null when API returns alerts.");
            Assert.AreEqual(apiResult.Alerts!.Alerts[0].Headline, result!.Alerts.First().Headline, "Alert headline mapping incorrect.");
        }
    }
}
