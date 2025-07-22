using ID.WeatherDashboard.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Elements
{
    public interface IElementService
    {
        Task StartAsync(DashboardViewModel viewModel);
    }
}
