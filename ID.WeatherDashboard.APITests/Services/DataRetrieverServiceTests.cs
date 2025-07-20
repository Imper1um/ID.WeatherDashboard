using Castle.Core.Logging;
using ID.WeatherDashboard.API.Codes;
using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.APITests.Services
{
    [TestClass]
    public class DataRetrieverServiceTests
    {
        protected List<Mock<ICurrentQueryService>> CurrentQueryServiceMocks = [];
        protected List<Mock<IForecastQueryService>> ForecastQueryServiceMocks = [];
        protected List<Mock<IHistoryQueryService>> HistoryQueryServiceMocks = [];
        protected List<Mock<ISunDataService>> SunDataServiceMocks = [];
        protected List<Mock<IAlertQueryService>> AlertQueryServiceMocks = [];
        protected Mock<IConfigManager> ConfigManagerMock = new();
        protected DashboardConfig Config = new();

        protected List<DataUpdatedEventArgs> CurrentDataUpdated = [];
        protected List<DataUpdatedEventArgs> ForecastDataUpdated = [];
        protected List<DataUpdatedEventArgs> HistoryDataUpdated = [];
        protected List<DataUpdatedEventArgs> SunDataUpdated = [];
        protected List<DataUpdatedEventArgs> AlertDataUpdated = [];

        [TestInitialize]
        public void Initialize()
        {
            CurrentQueryServiceMocks.Clear();
            ForecastQueryServiceMocks.Clear();
            HistoryQueryServiceMocks.Clear();
            AlertQueryServiceMocks.Clear();
            SunDataServiceMocks.Clear();
            CurrentDataUpdated.Clear();
            ForecastDataUpdated.Clear();
            HistoryDataUpdated.Clear();
            SunDataUpdated.Clear();
            AlertDataUpdated.Clear();
            ConfigManagerMock.Reset();
            Config = new DashboardConfig();
            ConfigManagerMock.SetupGet(m => m.Config).Returns(() => Config);
        }

        protected WeatherConditions GenerateWeatherConditions(DateTimeOffset? time = null,
            Precipitation? basePrecipitationRate = null,
            Precipitation? rainRate = null,
            Precipitation? snowRate = null,
            float? cloudCoverPercentage = null,
            API.Data.MoonPhase? moonPhase = null,
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
                BasePrecipitationRate = basePrecipitationRate,
                RainRate = rainRate,
                SnowRate = snowRate,
                CloudCoverPercentage = cloudCoverPercentage,
                MoonPhase = moonPhase,
                WindGustSpeed = windGustSpeed,
                WindSpeed = windSpeed,
                Visibility = visibility,
                SunAngle = sunAngle,
                MoonAngle = moonAngle,
                IsLightning = isLightning,
                IsFoggy = isFoggy,
                IsFreezing = isFreezing,
                IsHail = isHail,
                IsSleet = isSleet,
                IsRain = isRain,
                IsWarning = isWarning,
                IsHurricane = isHurricane,
                IsTornado = isTornado,
                StateConditions = stateConditions,
                Latitude = latitude
            };
        }

        protected WeatherConditions GenerateFullyFormedWeatherConditions(
            DateTimeOffset? time = null,
            Precipitation? basePrecipitationRate = null,
            Precipitation? rainRate = null,
            Precipitation? snowRate = null,
            float? cloudCoverPercentage = null,
            API.Data.MoonPhase? moonPhase = null,
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
            return GenerateWeatherConditions(
                time: time,
                basePrecipitationRate: basePrecipitationRate ?? new Precipitation(TestHelpers.RandomFloatBetween(0, 2), PrecipitationEnum.Inches),
                rainRate: rainRate ?? new Precipitation(TestHelpers.RandomFloatBetween(0, 2), PrecipitationEnum.Inches),
                snowRate: snowRate ?? new Precipitation(TestHelpers.RandomFloatBetween(0, 2), PrecipitationEnum.Inches),
                cloudCoverPercentage: cloudCoverPercentage ?? TestHelpers.RandomFloatBetween(0, 1),
                moonPhase: moonPhase ?? TestHelpers.RandomEnumValue<API.Data.MoonPhase>(),
                windGustSpeed: windGustSpeed ?? new WindSpeed(TestHelpers.RandomFloatBetween(0, 50), WindSpeedEnum.MilesPerHour),
                windSpeed: windSpeed ?? new WindSpeed(TestHelpers.RandomFloatBetween(0, 30), WindSpeedEnum.MilesPerHour),
                visibility: visibility ?? new Distance(TestHelpers.RandomFloatBetween(0, 10), DistanceEnum.Miles),
                sunAngle: sunAngle ?? TestHelpers.RandomFloatBetween(-90, 90),
                moonAngle: moonAngle ?? TestHelpers.RandomFloatBetween(-90, 90),
                isLightning: isLightning ?? (Random.Shared.Next(0, 2) == 1),
                isFoggy: isFoggy ?? (Random.Shared.Next(0, 2) == 1),
                isFreezing: isFreezing ?? (Random.Shared.Next(0, 2) == 1),
                isHail: isHail ?? (Random.Shared.Next(0, 2) == 1),
                isSleet: isSleet ?? (Random.Shared.Next(0, 2) == 1),
                isRain: isRain ?? (Random.Shared.Next(0, 2) == 1),
                isWarning: isWarning ?? (Random.Shared.Next(0, 2) == 1),
                isHurricane: isHurricane ?? (Random.Shared.Next(0, 2) == 1),
                isTornado: isTornado ?? (Random.Shared.Next(0, 2) == 1),
                stateConditions: stateConditions ?? TestHelpers.RandomString(10, TestHelpers.UppercaseLetters),
                latitude: latitude ?? TestHelpers.RandomDoubleBetween(-90, 90));
        }

        protected DataRetrieverService GetDataRetriever()
        {
            var dr = new DataRetrieverService(CurrentQueryServiceMocks.Select(m => m.Object),
                ForecastQueryServiceMocks.Select(m => m.Object),
                HistoryQueryServiceMocks.Select(m => m.Object),
                SunDataServiceMocks.Select(m => m.Object),
                AlertQueryServiceMocks.Select(m => m.Object),
                ConfigManagerMock.Object);
            dr.CurrentDataUpdated += (sender, l) => CurrentDataUpdated.Add(l);
            dr.ForecastDataUpdated += (sender, l) => ForecastDataUpdated.Add(l);
            dr.HistoryDataUpdated += (sender, l) => HistoryDataUpdated.Add(l);
            dr.SunDataUpdated += (sender, l) => SunDataUpdated.Add(l);
            dr.AlertDataUpdated += (sender, l) => AlertDataUpdated.Add(l);
            return dr;
        }

        protected DataConfig GenerateAllStarConfig(params string[] serviceNames)
        {
            return new DataConfig()
            {
                OverlayExistingData = true,
                MaxDataAge = TimeSpan.FromMinutes(5),
                Elements =
                [
                    new ElementConfig()
                    {
                        Name = "*",
                        ServiceElements = serviceNames.Select(n => new ServiceElementConfig() { Action = "Override", ServiceName = n, Weight = 100}).ToList()
                    }
                ]
            };
        }
    }
}
