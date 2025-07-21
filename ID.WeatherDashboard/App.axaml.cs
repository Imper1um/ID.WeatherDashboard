using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Reflection;

namespace ID.WeatherDashboard;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog();
        });
        services.AddSingleton<IConfigManager, ConfigManager>();

        services.AddSingleton<IEventControllerService, EventControllerService>();
        services.AddSingleton<ILocationStorageService, LocationStorageService>();
        services.AddScoped<IJsonQueryService, JsonQueryService>();
        services.AddSingleton<IDataRetrieverService, DataRetrieverService>();
        services.AddSingleton<IEncoderService, EncoderService>();
        services.AddSingleton<IEventControllerService, EventControllerService>();

        var provider = services.BuildServiceProvider();
        var configManager = provider.GetRequiredService<IConfigManager>();
        var eventControllerService = provider.GetRequiredService<IEventControllerService>();

        configManager.Load();
        eventControllerService.Start();

        foreach (var s in configManager.Config.Services)
        {
            var assembly = Assembly.Load(s.Assembly);
            var t = assembly.GetType(s.Type);
            if (t == null)
            {
                throw new InvalidOperationException($"Could not find {s.Type} in {s.Assembly}. Check your configuration.");
            }

            var instance = ActivatorUtilities.CreateInstance(provider, t);
            var baseService = instance as BaseService ?? throw new InvalidOperationException($"{s.Type} in {s.Assembly} does not inherit from {nameof(BaseService)}, which is required.");
            baseService.SetServiceConfig(s);

            if (instance is ICurrentQueryService currentQueryService)
                services.AddSingleton(typeof(ICurrentQueryService), currentQueryService);
            if (instance is IForecastQueryService forecastQueryService)
                services.AddSingleton(typeof(IForecastQueryService), forecastQueryService);
            if (instance is IHistoryQueryService historyQueryService)
                services.AddSingleton(typeof(IHistoryQueryService), historyQueryService);
            if (instance is ISunDataService sunDataService)
                services.AddSingleton(typeof(ISunDataService), sunDataService);
            if (instance is IAlertQueryService alertQueryService)
                services.AddSingleton(typeof(IAlertQueryService), alertQueryService);
        }

        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                //DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                //DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
