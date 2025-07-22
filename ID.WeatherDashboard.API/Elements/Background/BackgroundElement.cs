using ID.WeatherDashboard.API.Codes;
using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Events;
using ID.WeatherDashboard.API.Services;
using ID.WeatherDashboard.API.ViewModels;
using Microsoft.Extensions.Logging;

namespace ID.WeatherDashboard.API.Elements.Background
{
    public class BackgroundElement(
            IDataRetrieverService dataRetrieverService,
            IEventControllerService eventControllerService,
            IEncoderService encoderService,
            IConfigManager configManager,
            ILogger<BackgroundElement> logger
        )
        : IElementService
    {
        private readonly IDataRetrieverService DataRetrieverService = dataRetrieverService;
        private readonly IEventControllerService EventControllerService = eventControllerService;
        private readonly IEncoderService EncoderService = encoderService;
        private readonly IConfigManager ConfigManager = configManager;
        private readonly ILogger<BackgroundElement> Log = logger;

        private static readonly string[] AcceptableExtensions = [".png", ".jpg", ".jpeg"];

        private readonly List<FileWeatherData> Cache = [];

        private readonly Queue<string> RecentImages = new();
        private const int RecentImageCacheSize = 3;
        private DateTimeOffset? LastBackgroundChange = null;
        private readonly TimeSpan MinimumBackgroundChangeLimit = TimeSpan.FromMinutes(2);

        private DashboardViewModel ViewModel { get; set; } = null!;

        public async Task StartAsync(DashboardViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(ConfigManager.Config.Background.BackgroundFolder))
                return;

            ViewModel = viewModel;
            await ChangeBackground();
        }

        private async Task ChangeBackground()
        {
            var currentData = await DataRetrieverService.GetCurrentDataAsync(ViewModel.Location);

            var backgroundFolderDirectory = new DirectoryInfo(ConfigManager.Config.Background.BackgroundFolder);
            var files = from f in backgroundFolderDirectory.GetFiles()
                        join ae in AcceptableExtensions on f.Extension.ToLower() equals ae
                        select f;

            foreach (var f in files)
            {
                var cacheItem = Cache.FirstOrDefault(c => c.Path == f.FullName);
                if (cacheItem != null && cacheItem.Modified == f.LastWriteTime && cacheItem.Size == f.Length)
                    continue;
                if (cacheItem != null)
                    Cache.Remove(cacheItem);
                cacheItem = new FileWeatherData()
                {
                    Path = f.FullName,
                    Modified = f.LastWriteTime,
                    Size = f.Length,
                    ImageWeatherData = await EncoderService.GetCurrentImageWeatherDataAsync(f.FullName)
                };
                Cache.Add(cacheItem);
            }

            if (currentData != null && (LastBackgroundChange == null || LastBackgroundChange?.Add(MinimumBackgroundChangeLimit) > DateTimeOffset.Now))
            {
                var iwd = GetCurrentExpectedConditions(currentData);

                var matchingImages = from c in Cache
                                     where c.ImageWeatherData != null
                                     where !RecentImages.Contains(c.Path)
                                     select new
                                     {
                                         Item = c,
                                         Score = GetDistanceScore(c.ImageWeatherData!, iwd)
                                     };

                FileWeatherData? selectedFileWeatherData = null;
                if (matchingImages.Count() < 5)
                {
                    var allCache = Cache.ToList();
                    selectedFileWeatherData = allCache[Random.Shared.Next(allCache.Count)];
                    Log.LogInformation($"There are less than 5 images with ImageWeatherData in the folder. This will be a random image.");
                } 
                else
                {
                    var closestImages = matchingImages.OrderByDescending(mi => mi.Score).Take(5).ToList();
                    var selected = closestImages[Random.Shared.Next(closestImages.Count())];
                    selectedFileWeatherData = selected.Item;
                }

                    
                RecentImages.Enqueue(selectedFileWeatherData.Path);
                if (RecentImages.Count > RecentImageCacheSize)
                    RecentImages.Dequeue();

                LastBackgroundChange = DateTimeOffset.Now;
                ViewModel.BackgroundSelection = new BackgroundSelection()
                {
                    Path = new Uri(selectedFileWeatherData.Path).AbsoluteUri,
                    WeatherData = selectedFileWeatherData.ImageWeatherData
                };
            }

            EventControllerService.AddEvent(new EventAction(OnTrigger, new DelayEventTrigger(TimeSpan.FromMinutes(5)), new UpdatedCurrentDataTrigger(DataRetrieverService, ViewModel.Location)));
        }

        private float GetDistanceScore(ImageWeatherData a, ImageWeatherData b)
        {
            float score = 0f;

            score += Math.Abs(a.CloudCover - b.CloudCover);
            score += Math.Abs(a.Rain - b.Rain);
            score += Math.Abs(a.Fog - b.Fog);
            score += Math.Abs(a.Lightning - b.Lightning);
            score += Math.Abs(a.Wind - b.Wind);
            score += Math.Abs(a.Extreme - b.Extreme);
            score += Math.Abs(a.Snow - b.Snow);

            var timeA = GetMidTime(a.MinTimeOfDay, a.MaxTimeOfDay);
            var timeB = GetMidTime(b.MinTimeOfDay, b.MaxTimeOfDay);
            var timeDiff = Math.Abs((timeA - timeB).TotalMinutes);
            score += (float)(timeDiff / 60.0);

            return score;
        }

        private TimeSpan GetMidTime(TimeSpan min, TimeSpan max)
        {
            if (min <= max)
            {
                return min + TimeSpan.FromTicks((max - min).Ticks / 2);
            }
            else
            {
                var totalMinutes = (TimeSpan.FromHours(24).TotalMinutes - min.TotalMinutes) + max.TotalMinutes;
                var halfMinutes = totalMinutes / 2;
                var resultMinutes = (min.TotalMinutes + halfMinutes) % TimeSpan.FromHours(24).TotalMinutes;
                return TimeSpan.FromMinutes(resultMinutes);
            }
        }


        private ImageWeatherData GetCurrentExpectedConditions(CurrentData currentData)
        {
            float cloudCover = currentData.WeatherConditions?.CloudCoverPercentage ?? 0f;
            float rain = 0f;
            float fog = currentData.WeatherConditions?.IsFoggy == true ? 1f : 0f;
            float lightning = currentData.WeatherConditions?.IsLightning == true ? 1f : 0f;
            float wind = 0f;
            float extreme = (currentData.WeatherConditions?.IsHurricane == true
                 || currentData.WeatherConditions?.IsTornado == true
                 || currentData.WeatherConditions?.IsWarning == true) ? 1f : 0f;
            float snow = 0f;
            TimeSpan minTimeOfDay = TimeSpan.FromHours(12);
            TimeSpan maxTimeOfDay = TimeSpan.FromHours(12);

            if (currentData.WeatherConditions?.IsRain == true)
            {
                rain = 0.5f;
                if (currentData.WeatherConditions.RainRate?.Amount > 0)
                    rain = Math.Clamp((currentData.WeatherConditions.RainRate.Amount ?? 0f) * 5f, 0.5f, 1.0f);
            }

            var gust = currentData.WeatherConditions?.WindGustSpeed?.To(WindSpeedEnum.MilesPerHour) ?? 0f;
            var windSpeed = currentData.WeatherConditions?.WindSpeed?.To(WindSpeedEnum.MilesPerHour) ?? 0f;
            var windValue = Math.Max(gust, windSpeed);
            wind = Math.Clamp(windValue / 50f, 0f, 1f);

            if (currentData.WeatherConditions?.SnowRate?.Amount > 0)
            {
                snow = Math.Clamp((currentData.WeatherConditions.SnowRate.Amount ?? 0f) * 5f, 0.5f, 1.0f);
            }

            if (currentData.WeatherConditions?.SunAngle > 6)
            {
                minTimeOfDay = TimeSpan.FromHours(9);
                maxTimeOfDay = TimeSpan.FromHours(18);
            }
            else if (currentData.WeatherConditions?.SunAngle < -18)
            {
                minTimeOfDay = TimeSpan.FromHours(21);
                maxTimeOfDay = TimeSpan.FromHours(5);
            }
            else if (currentData.Observed?.Hour > 12 && currentData.WeatherConditions?.SunAngle != null)
            {
                minTimeOfDay = maxTimeOfDay = CalculateSunsetHypotheticalTime(currentData.WeatherConditions.SunAngle ?? 0);
            } 
            else if (currentData.Observed?.Hour < 12 && currentData.WeatherConditions?.SunAngle != null)
            {
                minTimeOfDay = maxTimeOfDay = CalculateSunriseHypotheticalTime(currentData.WeatherConditions.SunAngle ?? 0);
            }

            return new ImageWeatherData()
            {
                CloudCover = cloudCover,
                Rain = rain,
                Fog = fog,
                Wind = wind,
                Extreme = extreme,
                Lightning = lightning,
                Snow = snow,
                MinTimeOfDay = minTimeOfDay,
                MaxTimeOfDay = maxTimeOfDay,
                Description = ""
            };
        }

        private TimeSpan CalculateSunsetHypotheticalTime(float sunAngle)
        {
            var sunsetStart = TimeSpan.FromHours(18);
            const float maxAngle = 6f;
            const float minAngle = -18f;
            const double totalMinutes = 120.0;
            const float totalDegrees = maxAngle - minAngle;

            var clampedAngle = Math.Clamp(sunAngle, minAngle, maxAngle);

            var degreesFromMax = maxAngle - clampedAngle;

            var minutesOffset = (degreesFromMax / totalDegrees) * totalMinutes;
            return sunsetStart.Add(TimeSpan.FromMinutes(minutesOffset));
        }

        private TimeSpan CalculateSunriseHypotheticalTime(float sunAngle)
        {
            var sunriseStart = TimeSpan.FromHours(6);
            const float minAngle = -18f;
            const float maxAngle = 6f;
            const double totalMinutes = 180.0;
            const float totalDegrees = maxAngle - minAngle;

            var clampedAngle = Math.Clamp(sunAngle, minAngle, maxAngle);

            var degreesFromMin = clampedAngle - minAngle;

            var minutesOffset = (degreesFromMin / totalDegrees) * totalMinutes;
            return sunriseStart.Add(TimeSpan.FromMinutes(minutesOffset));
        }


        private void OnTrigger(EventAction action, EventTrigger trigger)
        {
            Task.Run(() => ChangeBackground());
        }
    }
}
