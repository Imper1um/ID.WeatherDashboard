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

using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ID.WeatherDashboard;

internal sealed partial class Program
{
    private static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(c =>
            {
                var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
                c.AddJsonFile("serilog.json")
                 .AddJsonFile($"serilog.{env}.json", true);
            })
            .UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration))
            .ConfigureServices(s => s.AddLogging())
            .Build();

        await BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>();
}
