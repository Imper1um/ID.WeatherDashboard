using ID.WeatherDashboard.API.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.APITests.Services
{
    public class DataRetrieverServiceTests_Forecast : DataRetrieverServiceTests
    {
        private Mock<IForecastQueryService> SetupForecastQueryService(string name)
        {
            var s = new Mock<IForecastQueryService>();
            s.SetupGet(m => m.ServiceName).Returns(name);
            ForecastQueryServiceMocks.Add(s);
            return s;
        }
    }
}
