﻿using ID.WeatherDashboard.API.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Services
{
    public interface IQueryService
    {
        string ServiceName { get; }
        void SetServiceConfig(ServiceConfig config);
    }
}
