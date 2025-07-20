using ID.WeatherDashboard.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Config
{
    public class ChatGptConfig
    {
        public required string ApiKey { get; set; }
        public required string Model { get; set; }
    }
}
