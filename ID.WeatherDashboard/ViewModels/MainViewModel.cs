/*
 * ID.WeatherDashboard: Designed to give you your weather from your personal weather station.
    Copyright © 2025  Zachare Sylvestre (Imper1um)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Services;
using ReactiveUI;

namespace ID.WeatherDashboard.ViewModels;

public partial class MainViewModel(IDataRetrieverService dataRetrieverService, IConfigManager configManager) : ReactiveObject
{
    private readonly IDataRetrieverService DataRetrieverService = dataRetrieverService;
    private readonly IConfigManager ConfigManager = configManager;

    public string Greeting => "Welcome to Avalonia!";
}
