using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.MoonPhase.Data;
using ID.WeatherDashboard.MoonPhase.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.APITests.Services
{
    [TestClass]
    public class MoonPhaseServiceTests
    {
        private Mock<IJsonQueryService> JsonQueryService = null!;
        private MoonPhaseService MoonPhaseService = null!;

        [TestInitialize]
        public void Initialize()
        {
            JsonQueryService = new Mock<IJsonQueryService>();
            MoonPhaseService = new MoonPhaseService(JsonQueryService.Object);
        }

        private MoonPhaseConfig SetConfig(string? apiKey = null, string? name = null, int maxCallsPerDay = 100, int maxCallsPerHour = 100)
        {
            var c = new MoonPhaseConfig()
            {
                ApiKey = apiKey ?? TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.Digits),
                Name = name ?? TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.Digits),
                MaxCallsPerDay = maxCallsPerDay,
                MaxCallsPerHour = maxCallsPerHour
            };
            MoonPhaseService.SetServiceConfig(c);
            return c;
        }

        #region Generation

        private MoonPhaseAdvancedAPI GenerateMoonPhaseAdvancedAPI(DateTime? forDateTime = null, DateTimeOffset? pulled = null, MoonPhaseSun? sun = null, MoonPhaseMoon? moon = null, MoonPhaseMoonPhases? moonPhases = null, MoonPhaseLocation? location = null)
        {
            return new MoonPhaseAdvancedAPI(forDateTime ?? DateTime.Today, pulled ?? DateTimeOffset.Now)
            {
                Sun = sun ?? GenerateMoonPhaseSun(),
                Moon = moon ?? GenerateMoonPhaseMoon(),
                MoonPhases = moonPhases ?? GenerateMoonPhaseMoonPhases(),
                Location = location ?? GenerateMoonPhaseLocation()
            };
        }

        private MoonPhaseAdvancedAPI GenerateFilledMoonPhaseAdvancedAPI(DateTime? forDateTime = null, DateTimeOffset? pulled = null, MoonPhaseSun? sun = null, MoonPhaseMoon? moon = null, MoonPhaseMoonPhases? moonPhases = null, MoonPhaseLocation? location = null)
        {
            return GenerateMoonPhaseAdvancedAPI(
                forDateTime ?? DateTime.Today,
                pulled ?? DateTimeOffset.Now,
                sun ?? GenerateFilledMoonPhaseSun(),
                moon ?? GenerateFilledMoonPhaseMoon(),
                moonPhases ?? GenerateFilledMoonPhaseMoonPhases(),
                location ?? GenerateFilledMoonPhaseLocation());
        }

        private MoonPhaseLocation GenerateMoonPhaseLocation(double? latitude = null, double? longitude = null)
        {
            return new MoonPhaseLocation()
            {
                Latitude = latitude,
                Longitude = longitude
            };
        }

        private MoonPhaseLocation GenerateFilledMoonPhaseLocation(double? latitude = null, double? longitude = null)
        {
            return GenerateMoonPhaseLocation(
                latitude ?? TestHelpers.RandomLatitude(),
                longitude ?? TestHelpers.RandomLongitude()
            );
        }

        private MoonPhaseSun GenerateMoonPhaseSun(string? sunrise = null, long? sunriseTimestamp = null, string? sunset = null, long? sunsetTimestamp = null, string? solarNoon = null, string? dayLength = null, double? sunAltitude = null, double? sunDistance = null, double? sunAzimuth = null, MoonPhaseNextSolarEclipse? nextSolarEclipse = null)
        {
            return new MoonPhaseSun()
            {
                Sunrise = sunrise,
                SunriseTimestamp = sunriseTimestamp,
                SunAltitude = sunAltitude,
                SunDistance = sunDistance,
                SunAzimuth = sunAzimuth,
                Sunset = sunset,
                SunsetTimestamp = sunsetTimestamp,
                SolarNoon = solarNoon,
                DayLength = dayLength,
                NextSolarEclipse = nextSolarEclipse ?? GenerateMoonPhaseNextSolarEclipse()
            };
        }

        private MoonPhaseSun GenerateFilledMoonPhaseSun(string? sunrise = null, long? sunriseTimestamp = null, string? sunset = null, long? sunsetTimestamp = null, string? solarNoon = null, string? dayLength = null, double? sunAltitude = null, double? sunDistance = null, double? sunAzimuth = null, MoonPhaseNextSolarEclipse? nextSolarEclipse = null)
        {
            var sunriseDt = DateTimeOffset.FromUnixTimeSeconds(sunriseTimestamp ?? TestHelpers.RandomDateTimeOffsetBetween(DateTimeOffset.Now.DayOf(), DateTimeOffset.Now.DayOf().AddHours(12)).ToUnixTimeSeconds());
            var sunsetDt = DateTimeOffset.FromUnixTimeSeconds(sunsetTimestamp ?? TestHelpers.RandomDateTimeOffsetBetween(DateTimeOffset.Now.DayOf(), DateTimeOffset.Now.DayOf().AddHours(12)).ToUnixTimeSeconds());
            var sunriseS = sunrise ?? sunriseDt.ToString("HH:mm");
            var sunsetS = sunset ?? sunsetDt.ToString("HH:mm");
            var solarNoonS = solarNoon ?? TestHelpers.RandomDateTimeOffsetBetween(sunriseDt, sunsetDt).ToString("HH:mm");
            var dayLengthS = dayLength ?? (sunsetDt - sunriseDt).ToString(@"hh\:mm");
            return GenerateMoonPhaseSun(
                sunriseS,
                sunriseDt.ToUnixTimeSeconds(),
                sunsetS,
                sunsetDt.ToUnixTimeSeconds(),
                solarNoonS,
                dayLengthS,
                sunAltitude ?? TestHelpers.RandomMoonAltitude(),
                sunDistance ?? TestHelpers.RandomSunDistance(),
                sunAzimuth ?? TestHelpers.RandomAzimuth(),
                nextSolarEclipse ?? GenerateFilledMoonPhaseNextSolarEclipse()
                );
        }

        private MoonPhaseNextSolarEclipse GenerateMoonPhaseNextSolarEclipse(string? type = null, string? visibilityRegions = null)
        {
            return new MoonPhaseNextSolarEclipse()
            {
                Type = type,
                VisibilityRegions = visibilityRegions
            };
        }

        private MoonPhaseNextSolarEclipse GenerateFilledMoonPhaseNextSolarEclipse(string? type = null, string? visibilityRegions = null)
        {
            return GenerateMoonPhaseNextSolarEclipse(
                type ?? TestHelpers.RandomName(),
                visibilityRegions ?? TestHelpers.RandomName()
            );
        }

        private MoonPhaseMoon GenerateMoonPhaseMoon(double? phase = null, string? phaseName = null, string? stage = null, string? illumination = null, int? ageDays = null, string? lunarCycle = null, string? emoji = null, MoonPhaseZodiac? zodiac = null, string? moonrise = null, long? moonriseTimestamp = null, string? moonset = null, long? moonsetTimestamp = null, double? moonAltitude = null, double? moonDistance = null, double? moonAzimuth = null, double? moonParallacticAngle = null, MoonPhaseNextLunarEclipse? nextLunarEclipse = null) 
        {
            return new MoonPhaseMoon()
            {
                Phase = phase,
                PhaseName = phaseName,
                Stage = stage,
                Illumination = illumination,
                AgeDays = ageDays,
                LunarCycle = lunarCycle,
                Emoji = emoji,
                Zodiac = zodiac ?? GenerateMoonPhaseZodiac(),
                Moonrise = moonrise,
                MoonriseTimestamp = moonriseTimestamp,
                Moonset = moonset,
                MoonsetTimestamp = moonsetTimestamp,
                MoonAltitude = moonAltitude,
                MoonDistance = moonDistance,
                MoonAzimuth = moonAzimuth,
                MoonParallacticAngle = moonParallacticAngle,
                NextLunarEclipse = nextLunarEclipse ?? GenerateMoonPhaseNextLunarEclipse()
            };
        }

        private MoonPhaseMoon GenerateFilledMoonPhaseMoon(double? phase = null, string? phaseName = null, string? stage = null, string? illumination = null, int? ageDays = null, string? lunarCycle = null, string? emoji = null, MoonPhaseZodiac? zodiac = null, string? moonrise = null, long? moonriseTimestamp = null, string? moonset = null, long? moonsetTimestamp = null, double? moonAltitude = null, double? moonDistance = null, double? moonAzimuth = null, double? moonParallacticAngle = null, MoonPhaseNextLunarEclipse? nextLunarEclipse = null)
        {
            var moonriseDt = DateTimeOffset.FromUnixTimeSeconds(moonriseTimestamp ?? TestHelpers.RandomDateTimeOffsetBetween(DateTimeOffset.Now.DayOf(), DateTimeOffset.Now.DayOf().AddHours(12)).ToUnixTimeSeconds());
            var moonsetDt = DateTimeOffset.FromUnixTimeSeconds(moonsetTimestamp ?? TestHelpers.RandomDateTimeOffsetBetween(DateTimeOffset.Now.DayOf().AddHours(12), DateTimeOffset.Now.DayOf().AddHours(24)).ToUnixTimeSeconds());
            var moonriseS = moonrise ?? moonriseDt.ToString("HH:mm");
            var moonsetS = moonset ?? moonsetDt.ToString("HH:mm");
            return GenerateMoonPhaseMoon(
                phase ?? Random.Shared.NextDouble(),
                phaseName ?? TestHelpers.RandomName(),
                stage ?? TestHelpers.RandomName(),
                illumination ?? TestHelpers.RandomName(),
                ageDays ?? TestHelpers.RandomIntBetween(1, 30),
                lunarCycle ?? TestHelpers.RandomName(),
                emoji ?? TestHelpers.RandomString(1, "🌑🌓🌔🌕🌒🌖🌗🌘".ToCharArray()),
                zodiac ?? GenerateFilledMoonPhaseZodiac(),
                moonriseS,
                moonriseDt.ToUnixTimeSeconds(),
                moonsetS,
                moonsetDt.ToUnixTimeSeconds(),
                moonAltitude ?? TestHelpers.RandomMoonAltitude(),
                moonDistance ?? TestHelpers.RandomMoonDistance(),
                moonAzimuth ?? TestHelpers.RandomAzimuth(),
                moonParallacticAngle ?? TestHelpers.RandomParallacticAngle(),
                nextLunarEclipse ?? GenerateFilledMoonPhaseNextLunarEclipse()
                );
        }

        private MoonPhaseZodiac GenerateMoonPhaseZodiac(string? sunSign = null, string? moonSign = null)
        {
            return new MoonPhaseZodiac()
            {
                SunSign = sunSign,
                MoonSign = moonSign
            };
        }

        private MoonPhaseZodiac GenerateFilledMoonPhaseZodiac(string? sunSign = null, string? moonSign = null)
        {
            return GenerateMoonPhaseZodiac(
                sunSign ?? TestHelpers.RandomName(), 
                moonSign ?? TestHelpers.RandomName()
            );
        }

        private MoonPhaseNextLunarEclipse GenerateMoonPhaseNextLunarEclipse(string? type = null, string? visibilityRegions = null)
        {
            return new MoonPhaseNextLunarEclipse()
            {
                Type = type,
                VisibilityRegions = visibilityRegions
            };
        }

        private MoonPhaseNextLunarEclipse GenerateFilledMoonPhaseNextLunarEclipse(string? type = null, string? visibilityRegions = null)
        {
            return GenerateMoonPhaseNextLunarEclipse(
                type ?? TestHelpers.RandomName(),
                visibilityRegions ?? TestHelpers.RandomName());
        }

        private MoonPhaseMoonPhases GenerateMoonPhaseMoonPhases(MoonPhasePhase? newMoon = null, MoonPhasePhase? firstQuarter = null, MoonPhasePhase? fullMoon = null, MoonPhasePhase? lastQuarter = null)
        {
            return new MoonPhaseMoonPhases()
            {
                NewMoon = newMoon ?? GenerateMoonPhasePhase(),
                FirstQuarter = firstQuarter ?? GenerateMoonPhasePhase(),
                FullMoon = fullMoon ?? GenerateMoonPhasePhase(),
                LastQuarter = lastQuarter ?? GenerateMoonPhasePhase()
            };
        }

        private MoonPhaseMoonPhases GenerateFilledMoonPhaseMoonPhases(MoonPhasePhase? newMoon = null, MoonPhasePhase? firstQuarter = null, MoonPhasePhase? fullMoon = null, MoonPhasePhase? lastQuarter = null)
        {
            return GenerateMoonPhaseMoonPhases(
                newMoon ?? GenerateFilledMoonPhasePhase(),
                firstQuarter ?? GenerateFilledMoonPhasePhase(),
                fullMoon ?? GenerateFilledMoonPhasePhase(),
                lastQuarter ?? GenerateFilledMoonPhasePhase()
                );
        }

        private MoonPhasePhase GenerateMoonPhasePhase(MoonPhaseLast? last = null, MoonPhaseNext? next = null) 
        {
            return new MoonPhasePhase()
            {
                Last = last ?? GenerateMoonPhaseLast(),
                Next = next ?? GenerateMoonPhaseNext()
            };
        }

        private MoonPhasePhase GenerateFilledMoonPhasePhase(MoonPhaseLast? last = null, MoonPhaseNext? next = null)
        {
            return GenerateMoonPhasePhase(
                last ?? GenerateFilledMoonPhaseLast(),
                next ?? GenerateFilledMoonPhaseNext());
        }

        private MoonPhaseNext GenerateMoonPhaseNext(int? daysAhead = null, string? name = null, string? description = null)
        {
            return new MoonPhaseNext()
            {
                DaysAhead = daysAhead,
                Name = name,
                Description = description
            };
        }

        private MoonPhaseNext GenerateFilledMoonPhaseNext(int? daysAhead = null, string? name = null, string? description = null)
        {
            return GenerateMoonPhaseNext(
                daysAhead ?? TestHelpers.RandomIntBetween(1, 30),
                name ?? TestHelpers.RandomName(),
                description ?? TestHelpers.RandomName()
            );
        }

        private MoonPhaseLast GenerateMoonPhaseLast(int? daysAgo = null, string? name = null, string? description = null)
        {
            return new MoonPhaseLast()
            {
                DaysAgo = daysAgo,
                Name = name,
                Description = description
            };
        }

        private MoonPhaseLast GenerateFilledMoonPhaseLast(int? daysAgo = null, string? name = null, string? description = null)
        {
            return GenerateMoonPhaseLast(
                daysAgo ?? TestHelpers.RandomIntBetween(1, 30),
                name ?? TestHelpers.RandomName(),
                description ?? TestHelpers.RandomName()
            );
        }

        #endregion

        private void VerifyServiceRun(MoonPhaseConfig config, string? url, Tuple<string, string>[]? headers, Location location, DateTimeOffset date, SunData? result, Times? times = null)
        {
            var t = times ?? Times.Once();
            JsonQueryService.Verify(j => j.QueryAsync<MoonPhaseAdvancedAPI>(
                It.IsAny<string>(),
                It.IsAny<Tuple<string, string>[]>()), t);
            Assert.IsNotNull(url, $"{nameof(url)} was not set.");
            Assert.IsNotNull(headers, $"{nameof(headers)} was not set.");
            Assert.IsTrue(headers.Any(h => h.Item1 == "x-rapidapi-host"), $"x-rapidapi-host was not set in {nameof(headers)}.");
            Assert.IsTrue(headers.Any(h => h.Item1 == "x-rapidapi-key"), $"x-rapidapi-key was not set in {nameof(headers)}.");
            Assert.IsTrue(headers.Any(h => h.Item1 == "x-rapidapi-host" && h.Item2 == "moon-phase.p.rapidapi.com"), $"x-rapidapi-host was not set to the correct host.");
            Assert.IsTrue(headers.Any(h => h.Item1 == "x-rapidapi-key" && h.Item2 == config.ApiKey), $"x-rapidapi-key was not set to the correct apikey.");
            Assert.IsTrue(url.Contains($"lat={location.Latitude:F3}"), $"{nameof(url)} did not query the right latitude. Expected: lat={location.Latitude:F3}, but it was {url}");
            Assert.IsTrue(url.Contains($"lon={location.Longitude:F3}"), $"{nameof(url)} did not query the right longitude. Expected: lng={location.Longitude:F3}, but it was {url}");
            Assert.IsTrue(url.Contains($"date={date:yyyy-MM-dd}"), $"{nameof(url)} did not include the date queried.");
            Assert.IsNotNull(result, "Result was null when it shouldn't have been.");
            Assert.IsTrue(result.Lines.Any(), "SunData should contain at least one SunLine.");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ValidLocation_ReturnsSunDataWithLines()
        {
            var config = SetConfig();

            var testDate = DateTimeOffset.Now.Date;
            var testApiResponse = GenerateFilledMoonPhaseAdvancedAPI();
            var location = new Location(testApiResponse.Location!.Latitude!.Value, testApiResponse.Location!.Longitude!.Value);

            string? url = null;
            Tuple<string, string>[]? headers = null;
            JsonQueryService
                .Setup(j => j.QueryAsync<MoonPhaseAdvancedAPI>(
                    It.IsAny<string>(),
                    It.IsAny<Tuple<string, string>[]>()))
                .Callback((string u, Tuple<string, string>[] t) => { url = u; headers = t; })
                .ReturnsAsync(testApiResponse);

            var result = await MoonPhaseService.GetSunDataAsync(location, testDate, testDate);
            VerifyServiceRun(config, url, headers, location, testDate, result);

            Assert.IsTrue(result!.Lines.All(l => l.For.Date == testDate.Date), "All SunLines should match the query date");
        }

        [TestMethod]
        public async Task GetSunDataAsync_InvalidLocationWithNullLatLon_ReturnsNull()
        {
            var config = SetConfig();

            var testDate = DateTimeOffset.Now.Date;
            var location = new Location("");

            string? url = null;
            Tuple<string, string>[]? headers = null;
            JsonQueryService
                .Setup(j => j.QueryAsync<MoonPhaseAdvancedAPI>(
                    It.IsAny<string>(),
                    It.IsAny<Tuple<string, string>[]>()))
                .Callback((string u, Tuple<string, string>[] t) => { url = u; headers = t; })
                .ReturnsAsync((MoonPhaseAdvancedAPI?)null);

            var result = await MoonPhaseService.GetSunDataAsync(location, testDate, testDate);

            Assert.IsNull(result, "Result should be null when location has null latitude and longitude.");
            JsonQueryService.Verify(j => j.QueryAsync<MoonPhaseAdvancedAPI>(
                It.IsAny<string>(),
                It.IsAny<Tuple<string, string>[]>()), Times.Never, "QueryAsync should not be called with invalid location.");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ValidLocation_QueryReturnsNull_SkippedInResult()
        {
            var config = SetConfig();

            var testDate = DateTimeOffset.Now.Date;
            var location = new Location(40.7128, -74.0060);

            string? url = null;
            Tuple<string, string>[]? headers = null;

            JsonQueryService
                .Setup(j => j.QueryAsync<MoonPhaseAdvancedAPI>(
                    It.IsAny<string>(),
                    It.IsAny<Tuple<string, string>[]>()))
                .Callback((string u, Tuple<string, string>[] t) => { url = u; headers = t; })
                .ReturnsAsync((MoonPhaseAdvancedAPI?)null);

            var result = await MoonPhaseService.GetSunDataAsync(location, testDate, testDate);

            JsonQueryService.Verify(j => j.QueryAsync<MoonPhaseAdvancedAPI>(
                It.IsAny<string>(),
                It.IsAny<Tuple<string, string>[]>()), Times.Once);

            Assert.IsNotNull(result, "Resulting SunData should not be null even if no valid data is returned.");
            Assert.IsFalse(result.Lines.Any(), "SunData.Lines should be empty when all API calls return null.");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ValidLocation_CallsJsonQueryServiceOncePerDay()
        {
            var config = SetConfig();

            var startDate = DateTimeOffset.Now.Date;
            var endDate = startDate.AddDays(2);
            var location = new Location(34.0522, -118.2437);

            int callCount = 0;

            JsonQueryService
                .Setup(j => j.QueryAsync<MoonPhaseAdvancedAPI>(
                    It.IsAny<string>(),
                    It.IsAny<Tuple<string, string>[]>()))
                .Callback(() => callCount++)
                .ReturnsAsync(GenerateFilledMoonPhaseAdvancedAPI());

            var result = await MoonPhaseService.GetSunDataAsync(location, startDate, endDate);

            Assert.IsNotNull(result, "Resulting SunData should not be null");
            Assert.AreEqual(3, callCount, "JsonQueryService.QueryAsync should be called once per day in the range");
            Assert.AreEqual(3, result.Lines.Count(), "SunData should contain one SunLine per successful query (one per day)");
        }

        [TestMethod]
        public async Task GetSunDataAsync_TryCallFails_DoesNotCallJsonQueryService()
        {
            var config = SetConfig();

            var testDate = DateTimeOffset.Now.Date;
            var location = new Location(51.5074, -0.1278);

            config.MaxCallsPerDay = 0;
            config.MaxCallsPerHour = 0;
            MoonPhaseService.SetServiceConfig(config);

            JsonQueryService
                .Setup(j => j.QueryAsync<MoonPhaseAdvancedAPI>(
                    It.IsAny<string>(),
                    It.IsAny<Tuple<string, string>[]>()))
                .ReturnsAsync(GenerateFilledMoonPhaseAdvancedAPI());

            var result = await MoonPhaseService.GetSunDataAsync(location, testDate, testDate);

            JsonQueryService.Verify(j => j.QueryAsync<MoonPhaseAdvancedAPI>(
                It.IsAny<string>(),
                It.IsAny<Tuple<string, string>[]>()), Times.Never, "QueryAsync should not be called if TryCall() fails");

            Assert.IsNotNull(result, "Resulting SunData should not be null even if no calls were made");
            Assert.IsFalse(result.Lines.Any(), "SunData should have no lines if TryCall() prevents execution");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ValidLocation_HeadersAreCorrectlyPassed()
        {
            var config = SetConfig();

            var testDate = DateTimeOffset.Now.Date;
            var latitude = TestHelpers.RandomLatitude();
            var longitude = TestHelpers.RandomLongitude();
            var location = new Location(latitude, longitude);

            string? url = null;
            Tuple<string, string>[]? headers = null;

            JsonQueryService
                .Setup(j => j.QueryAsync<MoonPhaseAdvancedAPI>(
                    It.IsAny<string>(),
                    It.IsAny<Tuple<string, string>[]>()))
                .Callback((string u, Tuple<string, string>[] h) => { url = u; headers = h; })
                .ReturnsAsync(GenerateFilledMoonPhaseAdvancedAPI());

            var result = await MoonPhaseService.GetSunDataAsync(location, testDate, testDate);

            VerifyServiceRun(config, url, headers, location, testDate, result);
        }

        [TestMethod]
        public async Task GetSunDataAsync_ValidLocation_ReturnsAggregatedSunDataLinesForMultipleDays()
        {
            var config = SetConfig();

            var startDate = DateTimeOffset.Now.Date;
            var endDate = startDate.AddDays(2); // 3 days total
            var latitude = TestHelpers.RandomLatitude();
            var longitude = TestHelpers.RandomLongitude();
            var location = new Location(latitude, longitude);

            var datesQueried = new List<DateTimeOffset>();

            JsonQueryService
                .Setup(j => j.QueryAsync<MoonPhaseAdvancedAPI>(
                    It.IsAny<string>(),
                    It.IsAny<Tuple<string, string>[]>()))
                .Callback((string u, Tuple<string, string>[] _) =>
                {
                    // Extract date from URL query
                    var datePart = u.Split("date=")[1];
                    datesQueried.Add(DateTimeOffset.Parse(datePart));
                })
                .ReturnsAsync(GenerateFilledMoonPhaseAdvancedAPI());

            var result = await MoonPhaseService.GetSunDataAsync(location, startDate, endDate);

            Assert.IsNotNull(result, "Resulting SunData should not be null.");
            Assert.AreEqual(3, result.Lines.Count(), "Expected one SunLine per queried day.");

            var queriedDates = datesQueried.Select(d => d.Date).OrderBy(d => d).ToArray();
            var resultDates = result.Lines.Select(l => l.For.Date).OrderBy(d => d).ToArray();

            CollectionAssert.AreEqual(queriedDates, resultDates, "Each SunLine should correspond to a queried date.");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ValidLocation_AllDaysReturnNull_ReturnsEmptySunData()
        {
            var config = SetConfig();

            var startDate = DateTimeOffset.Now.Date;
            var endDate = startDate.AddDays(2);
            var latitude = TestHelpers.RandomLatitude();
            var longitude = TestHelpers.RandomLongitude();
            var location = new Location(latitude, longitude);

            JsonQueryService
                .Setup(j => j.QueryAsync<MoonPhaseAdvancedAPI>(
                    It.IsAny<string>(),
                    It.IsAny<Tuple<string, string>[]>()))
                .ReturnsAsync((MoonPhaseAdvancedAPI?)null);

            var result = await MoonPhaseService.GetSunDataAsync(location, startDate, endDate);

            JsonQueryService.Verify(j => j.QueryAsync<MoonPhaseAdvancedAPI>(
                It.IsAny<string>(),
                It.IsAny<Tuple<string, string>[]>()), Times.Exactly(3), "QueryAsync should still be called once per day");

            Assert.IsNotNull(result, "SunData should still be returned even if no data lines are present.");
            Assert.IsFalse(result.Lines.Any(), "SunData.Lines should be empty if all API responses are null.");
        }

        [TestMethod]
        public async Task GetSunDataAsync_ValidLocation_QueryThrowsException_SkipsThatDayAndContinues()
        {
            var config = SetConfig();

            var startDate = DateTimeOffset.Now.Date;
            var endDate = startDate.AddDays(2); // 3 days total
            var latitude = TestHelpers.RandomLatitude();
            var longitude = TestHelpers.RandomLongitude();
            var location = new Location(latitude, longitude);

            int callCount = 0;

            JsonQueryService
                .Setup(j => j.QueryAsync<MoonPhaseAdvancedAPI>(
                    It.IsAny<string>(),
                    It.IsAny<Tuple<string, string>[]>()))
                .Callback(() => callCount++)
                .ReturnsAsync(() =>
                {
                    if (callCount == 2)
                        throw new InvalidOperationException("Simulated API failure");
                    return GenerateFilledMoonPhaseAdvancedAPI();
                });

            SunData? result = null;
            Exception? caughtException = null;

            try
            {
                result = await MoonPhaseService.GetSunDataAsync(location, startDate, endDate);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.IsNull(caughtException, "Exception should not propagate to the caller.");
            Assert.IsNotNull(result, "SunData should still be returned even if an exception occurs for some days.");
            Assert.AreEqual(2, result!.Lines.Count(), "SunData should contain lines for successful days only.");
            JsonQueryService.Verify(j => j.QueryAsync<MoonPhaseAdvancedAPI>(
                It.IsAny<string>(),
                It.IsAny<Tuple<string, string>[]>()), Times.Exactly(3), "QueryAsync should be attempted for all days.");
        }

    }
}
