using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Elements;
using ID.WeatherDashboard.API.Elements.Background;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.API.ViewModels;
using ID.WeatherDashboard.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Location = ID.WeatherDashboard.API.Data.Location;

namespace ID.WeatherDashboard;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private ServiceCollection ConfigureServices()
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

        return services;

        
    }

    private void InitializeQueryServices(IServiceCollection services)
    {
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
    }

    private void InitializeElements(IServiceCollection services)
    {
        services.AddSingleton<IElementService, BackgroundElement>();
    }

    private DashboardViewModel InitializeViewModel(IServiceCollection services)
    {
        var provider = services.BuildServiceProvider();
        var configManager = provider.GetRequiredService<IConfigManager>();

        var location = configManager.Config.Location;
        Location l = new Location(location);
        if (location.Contains(','))
        {
            var parts = location.Split(",");
            if (parts.Length == 2 && double.TryParse(parts[0], out var latitude) && double.TryParse(parts[1], out var longitude))
                l = new Location(latitude, longitude);
        }
        var viewModel = new DashboardViewModel() { Location = l };

        var elements = provider.GetServices<IElementService>();
        foreach (var e in elements)
        {
            Task.Run(() => e.StartAsync(viewModel));
        }

        return viewModel;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        DashboardViewModel viewModel = new DashboardViewModel()
        {
            Location = new Location("OnFrameworkInitializationCompleted") { Latitude = 28.538336, Longitude = -81.379234 },
            BackgroundSelection = new BackgroundSelection()
            {
                Path = "file:///D:/Projects/ID.WeatherScreen/assets/backgrounds/Cloudy001.jpg",
                WeatherData = new ImageWeatherData() { Description = "Nothing" }
            },
            Date = DateTime.Now
        };

        if (!Design.IsDesignMode)
        {
            var services = ConfigureServices();
            InitializeQueryServices(services);
            InitializeElements(services);
            viewModel = InitializeViewModel(services);

            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
        }


        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = viewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
