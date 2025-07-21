namespace ID.WeatherDashboard.API.Config
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System.Text.Json;

    public class ConfigManager : IConfigManager
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigManager> _logger;
        private string _configPath;
        public DashboardConfig Config { get; private set; } = null!;

        public ConfigManager(IConfiguration configuration, ILogger<ConfigManager> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _configPath = _configuration["WeatherDashboard:WeatherConfigPath"]
                          ?? throw new InvalidOperationException("WeatherConfigPath not set in configuration.");

            Load();
        }

        public void Reload()
        {
            _logger.LogInformation("Reloading WeatherConfig from {Path}", _configPath);
            Load();
        }

        public void Load()
        {
            if (!File.Exists(_configPath))
                throw new FileNotFoundException($"Weather configuration file not found at '{_configPath}'.");

            var json = File.ReadAllText(_configPath);
            Config = JsonSerializer.Deserialize<DashboardConfig>(json)
                     ?? throw new InvalidOperationException("Invalid WeatherConfig.json content.");
        }

        public void Save()
        {
            if (Config == null)
                throw new InvalidOperationException("No configuration loaded to save.");

            var json = JsonSerializer.Serialize(Config, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_configPath, json);
            _logger.LogInformation("WeatherConfig saved to {Path}", _configPath);
        }
    }

}
