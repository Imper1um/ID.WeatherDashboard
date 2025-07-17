using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Config
{
    public interface IConfigManager
    {
        void Load();
        void Reload();
        void Save();
        DashboardConfig Config { get; }
    }
}
