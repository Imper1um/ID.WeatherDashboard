using ID.WeatherDashboard.API.Services;
using Moq;

namespace ID.WeatherDashboard.APITests.Services
{
    public class DataRetrieverServiceTests_History : DataRetrieverServiceTests
    {
        private Mock<IHistoryQueryService> SetupHistoryQueryService(string name)
        {
            var s = new Mock<IHistoryQueryService>();
            s.SetupGet(m => m.ServiceName).Returns(name);
            HistoryQueryServiceMocks.Add(s);
            return s;
        }
    }
}
