using ID.WeatherDashboard.API.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.APITests.Services
{
    public class DataRetrieverServiceTests_Current : DataRetrieverServiceTests
    {
        private Mock<ICurrentQueryService> SetupCurrentQueryService(string name)
        {
            var s = new Mock<ICurrentQueryService>();
            s.SetupGet(m => m.ServiceName).Returns(name);
            CurrentQueryServiceMocks.Add(s);
            return s;
        }
    }
}
