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
