using ID.WeatherDashboard.API.Codes;
using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using System.Text.RegularExpressions;

namespace ID.WeatherDashboard.API.Services
{
    public class DataRetrieverService : IDataRetrieverService
    {
        public DataRetrieverService(IEnumerable<ICurrentQueryService> currentQueryServices, 
            IEnumerable<IForecastQueryService> forecastQueryServices, 
            IEnumerable<IHistoryQueryService> historyQueryServices, 
            IEnumerable<ISunDataService> sunDataServices,
            IEnumerable<IAlertQueryService> alertQueryServices,
            IConfigManager configManager)
        {
            CurrentQueryServices = currentQueryServices;
            ForecastQueryServices = forecastQueryServices;
            HistoryQueryServices = historyQueryServices;
            SunDataServices = sunDataServices;
            AlertQueryServices = alertQueryServices;
            ConfigManager = configManager;
        }

        private IEnumerable<ICurrentQueryService> CurrentQueryServices { get; }
        private IEnumerable<IForecastQueryService> ForecastQueryServices { get; }
        private IEnumerable<IHistoryQueryService> HistoryQueryServices { get; }
        private IEnumerable<ISunDataService> SunDataServices { get; }
        private IEnumerable<IAlertQueryService> AlertQueryServices { get; }
        private IConfigManager ConfigManager { get; }

        private Dictionary<Location, CurrentData> _currentDataCache = new Dictionary<Location, CurrentData>();
        private Dictionary<Location, ForecastData> _forecastDataCache = new Dictionary<Location, ForecastData>();
        private Dictionary<Location, HistoryData> _historyDataCache = new Dictionary<Location, HistoryData>();
        private Dictionary<Location, SunData> _sunDataCache = new Dictionary<Location, SunData>();
        private Dictionary<Location, AlertData> _alertDataCache = new Dictionary<Location, AlertData>();

        public event EventHandler<DataUpdatedEventArgs>? CurrentDataUpdated;
        public event EventHandler<DataUpdatedEventArgs>? ForecastDataUpdated;
        public event EventHandler<DataUpdatedEventArgs>? HistoryDataUpdated;
        public event EventHandler<DataUpdatedEventArgs>? SunDataUpdated;
        public event EventHandler<DataUpdatedEventArgs>? AlertDataUpdated;

        protected virtual void OnCurrentDataUpdated(Location location)
        {
            CurrentDataUpdated?.Invoke(this, new DataUpdatedEventArgs(location));
        }

        protected virtual void OnForecastDataUpdated(Location location)
        {
            ForecastDataUpdated?.Invoke(this, new DataUpdatedEventArgs(location));
        }

        protected virtual void OnHistoryDataUpdated(Location location)
        {
            HistoryDataUpdated?.Invoke(this, new DataUpdatedEventArgs(location));
        }

        protected virtual void OnSunDataUpdated(Location location)
        {
            SunDataUpdated?.Invoke(this, new DataUpdatedEventArgs(location));
        }

        protected virtual void OnAlertDataUpdated(Location location)
        {
            AlertDataUpdated?.Invoke(this, new DataUpdatedEventArgs(location));
        }

        private bool IsDataStillValid(IPulledData? pulledData, DataConfig dataConfig) 
        {
            if (pulledData == null) return false;
            var pulled = pulledData.Pulled;
            if (dataConfig.MaxDataAge != null)
            {
                var maxAge = DateTimeOffset.Now.Subtract(dataConfig.MaxDataAge.Value);
                if (pulled < maxAge)
                {
                    return false;
                }
            }
            if (!string.IsNullOrWhiteSpace(dataConfig.RefreshEvent))
            {
                switch (dataConfig.RefreshEvent)
                {
                    case "NextHour":
                        var nextHour = new DateTimeOffset(pulled.Year, pulled.Month, pulled.Day, pulled.Hour, 0, 0, pulled.Offset).AddHours(1);
                        if (DateTimeOffset.Now >= nextHour)
                        {
                            return false;
                        }
                        break;
                    case "NextDay":
                        var nextDay = new DateTimeOffset(pulled.Year, pulled.Month, pulled.Day, 0, 0, 0, pulled.Offset).AddDays(1);
                        if (DateTimeOffset.Now >= nextDay)
                        {
                            return false;
                        }
                        break;
                }
            }
            return true;
        }

        private IEnumerable<ServiceElementConfig> GetServiceElementConfigsForProperty(string propertyName, DataConfig dataConfig)
        {
            var elements = dataConfig.Elements.Where(e => IsElementValidFor(e, propertyName));
            return elements.SelectMany(e => e.ServiceElements);
        }

        private bool IsElementValidFor(ElementConfig e, string propertyName)
        {
            if (string.IsNullOrEmpty(e.Name) || string.IsNullOrEmpty(propertyName))
                return false;

            var pattern = "^" + Regex.Escape(e.Name)
                .Replace(@"\*", ".*") + "$";
            return Regex.IsMatch(propertyName, pattern, RegexOptions.IgnoreCase);
        }

        public async Task<CurrentData?> GetCurrentDataAsync(Location location)
        {
            var data = _currentDataCache.FirstOrDefault(_currentDataCache => _currentDataCache.Key.Equals(location)).Value;
            var config = ConfigManager.Config.CurrentData;
            if (IsDataStillValid(data, config))
                return data;

            var servicesToQuery = config.Elements
                .SelectMany(element => element.ServiceElements)
                .Select(se => se.ServiceName)
                .Distinct();
            var serviceQueries = new Dictionary<string, CurrentData>();
            if (!servicesToQuery.Any())
            {
                throw new InvalidOperationException("No services to query for CurrentData mentioned in the Elements. Please check the configuration.");
            }
            foreach (var s in servicesToQuery)
            {
                var service = CurrentQueryServices.FirstOrDefault(q => string.Equals(q.ServiceName, s, StringComparison.OrdinalIgnoreCase));
                if (service == null)
                    throw new InvalidOperationException($"Service {s} does not exist, which was mentioned in CurrentData Elements.");

                var returnData = await service.GetCurrentDataAsync(location);
                if (returnData != null)
                {
                    serviceQueries.Add(s, returnData);
                }
            }
            if (!serviceQueries.Any())
            {
                throw new InvalidOperationException($"No data was retrieved from any services. Check to make sure your API keys are correct!");
            }

            var currentData = config.OverlayExistingData ? (data ?? new CurrentData(DateTimeOffset.Now)) : new CurrentData(DateTimeOffset.Now);
            currentData.Observed = serviceQueries.Max(sq => sq.Value.Observed ?? DateTimeOffset.MinValue);
            currentData.Sources = serviceQueries.Select(sq => sq.Value.Sources).SelectMany(s => s).Union(currentData.Sources).Distinct().ToArray();

            SetElement(config, currentData, serviceQueries, "StationId", (c, i) => c.StationId = i, c => c.StationId);
            SetElement(config, currentData, serviceQueries, "WindDirection", (c, i) => c.WindDirection = i, c => c.WindDirection);
            SetElement(config, currentData, serviceQueries, "Humidity", (c, i) => c.Humidity = i, c => c.Humidity);
            SetElement(config, currentData, serviceQueries, "CurrentTemperature", (c, i) => c.CurrentTemperature = i, c => c.CurrentTemperature);
            SetElement(config, currentData, serviceQueries, "FeelsLike", (c, i) => c.FeelsLike = i, c => c.FeelsLike);
            SetElement(config, currentData, serviceQueries, "HeatIndex", (c, i) => c.HeatIndex = i, c => c.HeatIndex);
            SetElement(config, currentData, serviceQueries, "DewPoint", (c, i) => c.DewPoint = i, c => c.DewPoint);
            SetElement(config, currentData, serviceQueries, "UVIndex", (c, i) => c.UVIndex = i, c => c.UVIndex);
            SetElement(config, currentData, serviceQueries, "Pressure", (c, i) => c.Pressure = i, c => c.Pressure);
            SetElement(config, currentData, serviceQueries, "Coordinates", (c, i) => c.Coordinates = i, c => c.Coordinates);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.Time", (CurrentData c, DateTimeOffset? i) => c.WeatherConditions!.Time = i ?? DateTimeOffset.Now, (CurrentData c) => c.WeatherConditions?.Time);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.BasePrecipitationRate", (CurrentData c, Precipitation? i) => c.WeatherConditions!.BasePrecipitationRate = i, (CurrentData c) => c.WeatherConditions?.BasePrecipitationRate);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.RainRate", (c, i) => c.WeatherConditions!.RainRate = i, c => c.WeatherConditions?.RainRate);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.SnowRate", (c, i) => c.WeatherConditions!.SnowRate = i, c => c.WeatherConditions?.SnowRate);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.CloudCoverPercentage", (c, i) => c.WeatherConditions!.CloudCoverPercentage = i, c => c.WeatherConditions?.CloudCoverPercentage);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.MoonPhase", (c, i) => c.WeatherConditions!.MoonPhase = i, c => c.WeatherConditions?.MoonPhase);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.WindGustSpeed", (c, i) => c.WeatherConditions!.WindGustSpeed = i, c => c.WeatherConditions?.WindGustSpeed);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.WindSpeed", (c, i) => c.WeatherConditions!.WindSpeed = i, c => c.WeatherConditions?.WindSpeed);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.Visibility", (c, i) => c.WeatherConditions!.Visibility = i, c => c.WeatherConditions?.Visibility);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.SunAngle", (c, i) => c.WeatherConditions!.SunAngle = i, c => c.WeatherConditions?.SunAngle);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.IsLightning", (c, i) => c.WeatherConditions!.IsLightning = i, c => c.WeatherConditions?.IsLightning);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.IsFoggy", (c, i) => c.WeatherConditions!.IsFoggy = i, c => c.WeatherConditions?.IsFoggy);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.IsFreezing", (c, i) => c.WeatherConditions!.IsFreezing = i, c => c.WeatherConditions?.IsFreezing);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.IsHail", (c, i) => c.WeatherConditions!.IsHail = i, c => c.WeatherConditions?.IsHail);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.IsSleet", (c, i) => c.WeatherConditions!.IsSleet = i, c => c.WeatherConditions?.IsSleet);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.IsRain", (c, i) => c.WeatherConditions!.IsRain = i, c => c.WeatherConditions?.IsRain);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.IsWarning", (c, i) => c.WeatherConditions!.IsWarning = i, c => c.WeatherConditions?.IsWarning);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.IsHurricane", (c, i) => c.WeatherConditions!.IsHurricane = i, c => c.WeatherConditions?.IsHurricane);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.IsTornado", (c, i) => c.WeatherConditions!.IsTornado = i, c => c.WeatherConditions?.IsTornado);
            SetElement(config, currentData, serviceQueries, "WeatherConditions.StateConditions", (c, i) => c.WeatherConditions!.StateConditions = i, c => c.WeatherConditions?.StateConditions);

            //Now fill with SunData and AlertData
            var sunData = await GetSunDataAsync(location);
            var alertData = await GetAlertDataAsync(location);

            OverlaySunData(currentData, sunData);
            OverlayAlertData(currentData, alertData);

            var historyData = await GetHistoryDataAsync(location);
            if (historyData != null)
            {
                historyData.AddLine(new HistoryLine(currentData));
            }

            if (_currentDataCache.ContainsKey(location))
            {
                _currentDataCache[location] = currentData;
            }
            else
            {
                _currentDataCache.Add(location, currentData);
            }
            OnCurrentDataUpdated(location);

            return currentData;
        }

        private void OverlayAlertData(CurrentData currentData, AlertData? alertData)
        {
            if (currentData.WeatherConditions == null || alertData?.Alerts == null)
                return;

            var alerts = alertData.Alerts.ToList();

            bool ContainsKeyword(string keyword) =>
                alerts.Any(a => a.Headline?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true);

            currentData.WeatherConditions.IsTornado = ContainsKeyword("tornado");
            currentData.WeatherConditions.IsHurricane = ContainsKeyword("hurricane") || ContainsKeyword("tropical storm");
            currentData.WeatherConditions.IsWarning = alerts.Any(a =>
                a.Severity == AlertSeverityEnum.Severe ||
                a.Urgency == AlertUrgencyEnum.Immediate ||
                ContainsKeyword("warning"));
        }

        private void OverlaySunData(CurrentData currentData, SunData? sunData)
        {
            if (sunData?.Lines.Any() != true || currentData.WeatherConditions?.Latitude == null)
                return;
            var sunLine = sunData.Lines.FirstOrDefault(l => l.For.Date == DateTime.Today);
            if (sunLine == null)
                return;
            currentData.WeatherConditions.SunAngle = GetSunDegreesAboveHorizon(currentData.WeatherConditions, sunLine);
            currentData.WeatherConditions.MoonAngle = sunLine.MoonData?.MoonAngleAt(DateTime.Now);
        }

        private float? GetSunDegreesAboveHorizon(WeatherConditions weatherConditions, SunLine sunLine)
        {
            var solarNoon = sunLine.SolarNoon;
            var latitude = weatherConditions.Latitude;
            if (solarNoon == null || latitude == null)
                return null;

            var dayOfYear = (double)weatherConditions.Time.DayOfYear;
            double threeSixty = 360;
            double yearLength = 365;


            var declinationDegrees = -23.4 * Math.Cos((threeSixty / yearLength * (dayOfYear + 10)).ToRadians());
            var declinationRadians = declinationDegrees.ToRadians();
            var phiRadians = latitude.Value.ToRadians();

            var timeDifference = (DateTime.Now - solarNoon.Value).TotalSeconds / 3600;
            var hourAngleDegrees = 15 * timeDifference;
            var hourAngleRadians = hourAngleDegrees.ToRadians();

            var sinElevation = (Math.Sin(phiRadians) * Math.Sin(declinationRadians)) +
                               (Math.Cos(phiRadians) * Math.Cos(declinationRadians) * Math.Cos(hourAngleRadians));

            return (float)Math.Asin(sinElevation).ToDegrees();
        }

        public async Task<ForecastData?> GetForecastDataAsync(Location location)
        {
            var data = _forecastDataCache.FirstOrDefault(_forecastDataCache => _forecastDataCache.Key.Equals(location)).Value;
            var config = ConfigManager.Config.ForecastData;
            if (IsDataStillValid(data, config))
                return data;

            var servicesToQuery = config.Elements
                .SelectMany(element => element.ServiceElements)
                .Select(se => se.ServiceName)
                .Distinct();
            var serviceQueries = new Dictionary<string, ForecastData>();
            if (!servicesToQuery.Any())
            {
                throw new InvalidOperationException("No services to query for ForecastData mentioned in the Elements. Please check the config.");
            }

            var forecastData = config.OverlayExistingData ? (data ?? new ForecastData(DateTimeOffset.Now)) : new ForecastData(DateTimeOffset.Now);

            var currentHour = DateTimeOffset.Now.HourOf();
            var maxHour = currentHour.AddHours(48).AddSeconds(-1);
            var timesToQuery = new List<DateTimeOffset>();
            var timesToHave = new List<DateTimeOffset>();
            for (var d = currentHour; d <= maxHour; d = d.AddHours(1))
            {
                timesToHave.Add(d);
                if (forecastData.Days.Any(day => day.Lines.Any(h => h.Observed == d)))
                    continue;
                timesToQuery.Add(d);
            }
            if (!timesToQuery.Any() && data != null)
            {
                data.Pulled = DateTimeOffset.Now;
                return data;
            } 
            else if (!timesToQuery.Any())
            {
                throw new InvalidOperationException("There is some kind of issue with ForecastData did not have any data but there was nothing to query.");
            }
            var minDate = timesToHave.Min();
            var maxDate = timesToHave.Max();

            foreach (var s in servicesToQuery)
            {
                var service = ForecastQueryServices.FirstOrDefault(q => string.Equals(q.ServiceName, s, StringComparison.OrdinalIgnoreCase));
                if (service == null)
                    throw new InvalidOperationException($"Service {s} does not exist, which was mentioned in HistoryData Elements.");

                var returnData = await service.GetForecastDataAsync(location, minDate.Date, maxDate.Date);
                if (returnData != null)
                {
                    serviceQueries.Add(s, returnData);
                }
            }
            if (!serviceQueries.Any())
            {
                throw new InvalidOperationException($"No data was retrieved from any services. Check to make sure your API keys are correct!");
            }

            forecastData.Sources = serviceQueries.SelectMany(sq => sq.Value.Days.SelectMany(d => d.Sources)).Union(forecastData.Sources).Distinct().ToArray();

            var observedTimes = timesToHave.Distinct().OrderBy(t => t);
            var lines = new List<ForecastLine>();

            foreach (var ot in observedTimes)
            {
                var baseLine = forecastData.Days.SelectMany(d => d.Lines).FirstOrDefault(l => l.Observed?.HourOf() == ot);

                if (baseLine == null)
                {
                    baseLine = new ForecastLine(DateTimeOffset.Now, forecastData.Sources) { Observed = ot };
                }

                var matchingServiceLines = serviceQueries
                    .Where(sq => sq.Value.Days.SelectMany(d => d.Lines).Any(l => l.Observed?.HourOf() == ot))
                    .ToDictionary(
                        sq => sq.Key,
                        sq => sq.Value.Days.SelectMany(d => d.Lines).First(l => l.Observed?.HourOf() == ot)
                    );

                var matchingWeatherConditions = matchingServiceLines.Where(msl => msl.Value.WeatherConditions != null)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => (WeatherConditions)kvp.Value.WeatherConditions!
                    );

                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.Pulled), (l, b) => l.Pulled = b, l => l.Pulled);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.Observed), (l, b) => l.Observed = b, l => l.Observed);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.StationId), (l, b) => l.StationId = b, l => l.StationId);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.WindDirection), (l, b) => l.WindDirection = b, l => l.WindDirection);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.Humidity), (l, b) => l.Humidity = b, l => l.Humidity);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.CurrentTemperature), (l, b) => l.CurrentTemperature = b, l => l.CurrentTemperature);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.FeelsLike), (l, b) => l.FeelsLike = b, l => l.FeelsLike);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.HeatIndex), (l, b) => l.HeatIndex = b, l => l.HeatIndex);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.DewPoint), (l, b) => l.DewPoint = b, l => l.DewPoint);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.UVIndex), (l, b) => l.UVIndex = b, l => l.UVIndex);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.Pressure), (l, b) => l.Pressure = b, l => l.Pressure);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.Coordinates), (l, b) => l.Coordinates = b, l => l.Coordinates);
                SetElement(config, baseLine, matchingServiceLines, nameof(ForecastLine.RainChance), (l, b) => l.RainChance = b, l => l.RainChance);
                SetElement(config, baseLine, matchingServiceLines, nameof(ForecastLine.SnowChance), (l, b) => l.SnowChance = b, l => l.SnowChance);

                var cond = baseLine.WeatherConditions ?? new WeatherConditions(baseLine.Observed);
                baseLine.WeatherConditions = cond;

                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.BasePrecipitationRate)}", (w, v) => w.BasePrecipitationRate = v, w => w.BasePrecipitationRate);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.RainRate)}", (w, v) => w.RainRate = v, w => w.RainRate);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.SnowRate)}", (w, v) => w.SnowRate = v, w => w.SnowRate);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.CloudCoverPercentage)}", (w, v) => w.CloudCoverPercentage = v, w => w.CloudCoverPercentage);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.MoonPhase)}", (w, v) => w.MoonPhase = v, w => w.MoonPhase);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.WindGustSpeed)}", (w, v) => w.WindGustSpeed = v, w => w.WindGustSpeed);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.WindSpeed)}", (w, v) => w.WindSpeed = v, w => w.WindSpeed);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.Visibility)}", (w, v) => w.Visibility = v, w => w.Visibility);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.SunAngle)}", (w, v) => w.SunAngle = v, w => w.SunAngle);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsLightning)}", (w, v) => w.IsLightning = v, w => w.IsLightning);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsFoggy)}", (w, v) => w.IsFoggy = v, w => w.IsFoggy);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsFreezing)}", (w, v) => w.IsFreezing = v, w => w.IsFreezing);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsHail)}", (w, v) => w.IsHail = v, w => w.IsHail);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsSleet)}", (w, v) => w.IsSleet = v, w => w.IsSleet);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsRain)}", (w, v) => w.IsRain = v, w => w.IsRain);

                lines.Add(baseLine);
            }

            var days = new List<ForecastDay>();
            var daysOf = observedTimes.GroupBy(ot => ot.DayOf()).Select(ot => ot.Key);
            foreach (var ot in daysOf)
            {
                var baseLine = forecastData.Days.FirstOrDefault(l => l.Observed?.DayOf() == ot);

                if (baseLine == null)
                {
                    baseLine = new ForecastDay(DateTimeOffset.Now, forecastData.Sources) { Observed = ot };
                }

                var matchingServiceLines = serviceQueries
                    .Where(sq => sq.Value.Days.Any(l => l.Observed?.DayOf() == ot))
                    .ToDictionary(
                        sq => sq.Key,
                        sq => sq.Value.Days.First(l => l.Observed?.DayOf() == ot)
                    );

                var matchingWeatherConditions = matchingServiceLines.Where(msl => msl.Value.WeatherConditions != null)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => (WeatherConditions)kvp.Value.WeatherConditions!
                    );

                var matchingDaytimeData = matchingServiceLines.Where(msl => msl.Value.Daytime != null)
                    .ToDictionary(
                        msl => msl.Key,
                        msl => (DaytimeData)msl.Value.Daytime!
                    );

                var matchingNighttimeData = matchingServiceLines.Where(msl => msl.Value.Nighttime != null)
                    .ToDictionary(
                        msl => msl.Key,
                        msl => (NighttimeData)msl.Value.Nighttime!
                    );

                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.Pulled), (l, b) => l.Pulled = b, l => l.Pulled);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.Observed), (l, b) => l.Observed = b, l => l.Observed);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.StationId), (l, b) => l.StationId = b, l => l.StationId);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.WindDirection), (l, b) => l.WindDirection = b, l => l.WindDirection);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.Humidity), (l, b) => l.Humidity = b, l => l.Humidity);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.CurrentTemperature), (l, b) => l.CurrentTemperature = b, l => l.CurrentTemperature);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.FeelsLike), (l, b) => l.FeelsLike = b, l => l.FeelsLike);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.HeatIndex), (l, b) => l.HeatIndex = b, l => l.HeatIndex);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.DewPoint), (l, b) => l.DewPoint = b, l => l.DewPoint);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.UVIndex), (l, b) => l.UVIndex = b, l => l.UVIndex);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.Pressure), (l, b) => l.Pressure = b, l => l.Pressure);
                SetElement(config, baseLine, matchingServiceLines, nameof(DataLine.Coordinates), (l, b) => l.Coordinates = b, l => l.Coordinates);
                SetElement(config, baseLine, matchingServiceLines, nameof(ForecastLine.RainChance), (l, b) => l.RainChance = b, l => l.RainChance);
                SetElement(config, baseLine, matchingServiceLines, nameof(ForecastLine.SnowChance), (l, b) => l.SnowChance = b, l => l.SnowChance);

                var cond = baseLine.WeatherConditions ?? new WeatherConditions(baseLine.Observed);
                baseLine.WeatherConditions = cond;

                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.BasePrecipitationRate)}", (w, v) => w.BasePrecipitationRate = v, w => w.BasePrecipitationRate);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.RainRate)}", (w, v) => w.RainRate = v, w => w.RainRate);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.SnowRate)}", (w, v) => w.SnowRate = v, w => w.SnowRate);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.CloudCoverPercentage)}", (w, v) => w.CloudCoverPercentage = v, w => w.CloudCoverPercentage);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.MoonPhase)}", (w, v) => w.MoonPhase = v, w => w.MoonPhase);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.WindGustSpeed)}", (w, v) => w.WindGustSpeed = v, w => w.WindGustSpeed);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.WindSpeed)}", (w, v) => w.WindSpeed = v, w => w.WindSpeed);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.Visibility)}", (w, v) => w.Visibility = v, w => w.Visibility);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.SunAngle)}", (w, v) => w.SunAngle = v, w => w.SunAngle);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsLightning)}", (w, v) => w.IsLightning = v, w => w.IsLightning);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsFoggy)}", (w, v) => w.IsFoggy = v, w => w.IsFoggy);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsFreezing)}", (w, v) => w.IsFreezing = v, w => w.IsFreezing);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsHail)}", (w, v) => w.IsHail = v, w => w.IsHail);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsSleet)}", (w, v) => w.IsSleet = v, w => w.IsSleet);
                SetElement(config, cond, matchingWeatherConditions, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsRain)}", (w, v) => w.IsRain = v, w => w.IsRain);

                var dtd = baseLine.Daytime ?? new DaytimeData();
                baseLine.Daytime = dtd;
                var ntd = baseLine.Nighttime ?? new NighttimeData();
                baseLine.Nighttime = ntd;

                SetElement(config, dtd, matchingDaytimeData, $"{nameof(ForecastDay.Daytime)}.{nameof(DaytimeData.Rain)}", (x, y) => x.Rain = y, x => x.Rain);
                SetElement(config, dtd, matchingDaytimeData, $"{nameof(ForecastDay.Daytime)}.{nameof(DaytimeData.RainPercentage)}", (x, y) => x.RainPercentage = y, x => x.RainPercentage);
                SetElement(config, dtd, matchingDaytimeData, $"{nameof(ForecastDay.Daytime)}.{nameof(DaytimeData.ForecastText)}", (x, y) => x.ForecastText = y, x => x.ForecastText);
                SetElement(config, dtd, matchingDaytimeData, $"{nameof(ForecastDay.Daytime)}.{nameof(DaytimeData.Snow)}", (x, y) => x.Snow = y, x => x.Snow);
                SetElement(config, dtd, matchingDaytimeData, $"{nameof(ForecastDay.Daytime)}.{nameof(DaytimeData.SnowPercentage)}", (x, y) => x.SnowPercentage = y, x => x.SnowPercentage);
                SetElement(config, dtd, matchingDaytimeData, $"{nameof(ForecastDay.Daytime)}.{nameof(DaytimeData.High)}", (x, y) => x.High = y, x => x.High);

                SetElement(config, ntd, matchingNighttimeData, $"{nameof(ForecastDay.Nighttime)}.{nameof(NighttimeData.Low)}", (x, y) => x.Low = y, x => x.Low);
                SetElement(config, ntd, matchingNighttimeData, $"{nameof(ForecastDay.Nighttime)}.{nameof(NighttimeData.ForecastText)}", (x, y) => x.ForecastText = y, x => x.ForecastText);

                baseLine.ReplaceLines(lines.Where(l => l.Observed?.DayOf() == ot));
                days.Add(baseLine);
            }

            forecastData.ReplaceLines(days);
            forecastData.Pulled = DateTimeOffset.Now;

            if (_forecastDataCache.ContainsKey(location))
            {
                _forecastDataCache[location] = forecastData;
            } else
            {
                _forecastDataCache.Add(location, forecastData);
            }
            OnForecastDataUpdated(location);
            return forecastData;
        }

        public async Task<HistoryData?> GetHistoryDataAsync(Location location)
        {
            var data = _historyDataCache.FirstOrDefault(_historyDataCache => _historyDataCache.Key.Equals(location)).Value;
            var config = ConfigManager.Config.HistoryData;
            if (IsDataStillValid(data, ConfigManager.Config.HistoryData))
                return data;

            var servicesToQuery = config.Elements
                .SelectMany(element => element.ServiceElements)
                .Select(se => se.ServiceName)
                .Distinct();
            var serviceQueries = new Dictionary<string, HistoryData>();
            if (!servicesToQuery.Any())
            {
                throw new InvalidOperationException("No services to query for HistoryData mentioned in the Elements. Please check the configuration.");
            }

            var historyData = config.OverlayExistingData ? (data ?? new HistoryData(DateTimeOffset.Now)) : new HistoryData(DateTimeOffset.Now);

            var datesToQuery = new List<DateTimeOffset>();
            var datesToHave = new List<DateTimeOffset>();
            for (var d = DateTimeOffset.Now.AddDays(-2); d <= DateTimeOffset.Now; d = d.AddDays(1))
            {
                if (historyData.Lines.Any(hd => hd.Observed?.Date == d.Date))
                    continue;
                datesToQuery.Add(d);
            }
            if (!datesToQuery.Any() && data != null)
            {
                data.Pulled = DateTimeOffset.Now;
                return data;
            }
            else if (!datesToQuery.Any())
            {
                throw new InvalidOperationException("There is some kind of issue with HistoryData did not have any data but there was nothing to query.");
            }
            var minDate = datesToQuery.Min();
            var maxDate = datesToQuery.Max();

            foreach (var s in servicesToQuery)
            {
                var service = HistoryQueryServices.FirstOrDefault(q => string.Equals(q.ServiceName, s, StringComparison.OrdinalIgnoreCase));
                if (service == null)
                    throw new InvalidOperationException($"Service {s} does not exist, which was mentioned in HistoryData Elements.");

                var returnData = await service.GetHistoryDataAsync(location, minDate.Date, maxDate.Date);
                if (returnData != null)
                {
                    serviceQueries.Add(s, returnData);
                }
            }
            if (!serviceQueries.Any())
            {
                throw new InvalidOperationException($"No data was retrieved from any services. Check to make sure your API keys are correct!");
            }

            historyData.Sources = serviceQueries.SelectMany(sq => sq.Value.Lines.SelectMany(l => l.Sources)).Union(historyData.Sources).Distinct().ToArray();

            var observedTimes = serviceQueries.SelectMany(q => q.Value.Lines.Where(l => l.Observed != null).Select(l => l.Observed)).Distinct();
            var lines = new List<HistoryLine>();
            foreach (var ot in observedTimes)
            {
                var baseLine = historyData.Lines.FirstOrDefault(l => l.Observed == ot);
                if (baseLine == null) baseLine = new HistoryLine(DateTimeOffset.Now);

                var t = serviceQueries.Where(sq => sq.Value.Lines.Any(l => l.Observed == ot)).ToDictionary(
                    sq => sq.Key,
                    sq => sq.Value.Lines.First(l => l.Observed == ot)
                    );
                var wc = t.ToDictionary(
                    sq => sq.Key,
                    sq => (WeatherConditions)sq.Value.WeatherConditions!
                    );

                SetElement(config, baseLine, t, nameof(DataLine.Pulled), (sl, b) => sl.Pulled = b, sl => sl.Pulled);
                SetElement(config, baseLine, t, nameof(DataLine.Observed), (sl, b) => sl.Observed = b, sl => sl.Observed);
                SetElement(config, baseLine, t, nameof(DataLine.StationId), (sl, b) => sl.StationId = b, sl => sl.StationId);
                SetElement(config, baseLine, t, nameof(DataLine.WindDirection), (sl, b) => sl.WindDirection = b, sl => sl.WindDirection);
                SetElement(config, baseLine, t, nameof(DataLine.Humidity), (sl, b) => sl.Humidity = b, sl => sl.Humidity);
                SetElement(config, baseLine, t, nameof(DataLine.CurrentTemperature), (sl, b) => sl.CurrentTemperature = b, sl => sl.CurrentTemperature);
                SetElement(config, baseLine, t, nameof(DataLine.FeelsLike), (sl, b) => sl.FeelsLike = b, sl => sl.FeelsLike);
                SetElement(config, baseLine, t, nameof(DataLine.HeatIndex), (sl, b) => sl.HeatIndex = b, sl => sl.HeatIndex);
                SetElement(config, baseLine, t, nameof(DataLine.DewPoint), (sl, b) => sl.DewPoint = b, sl => sl.DewPoint);
                SetElement(config, baseLine, t, nameof(DataLine.UVIndex), (sl, b) => sl.UVIndex = b, sl => sl.UVIndex);
                SetElement(config, baseLine, t, nameof(DataLine.Pressure), (sl, b) => sl.Pressure = b, sl => sl.Pressure);
                SetElement(config, baseLine, t, nameof(DataLine.Coordinates), (sl, b) => sl.Coordinates = b, sl => sl.Coordinates);
                var cond = baseLine.WeatherConditions ?? new WeatherConditions(baseLine.Observed);
                baseLine.WeatherConditions = cond;
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.BasePrecipitationRate)}", (sl, b) => sl.BasePrecipitationRate = b, sl => sl?.BasePrecipitationRate);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.RainRate)}", (sl, b) => sl.RainRate = b, sl => sl?.RainRate);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.SnowRate)}", (sl, b) => sl.SnowRate = b, sl => sl?.SnowRate);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.CloudCoverPercentage)}", (sl, b) => sl.CloudCoverPercentage = b, sl => sl?.CloudCoverPercentage);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.MoonPhase)}", (sl, b) => sl.MoonPhase = b, sl => sl?.MoonPhase);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.WindGustSpeed)}", (sl, b) => sl.WindGustSpeed = b, sl => sl?.WindGustSpeed);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.WindSpeed)}", (sl, b) => sl.WindSpeed = b, sl => sl?.WindSpeed);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.Visibility)}", (sl, b) => sl.Visibility = b, sl => sl?.Visibility);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.SunAngle)}", (sl, b) => sl.SunAngle = b, sl => sl?.SunAngle);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.MoonAngle)}", (sl, b) => sl.MoonAngle = b, sl => sl?.MoonAngle);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsLightning)}", (sl, b) => sl.IsLightning = b, sl => sl?.IsLightning);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsFoggy)}", (sl, b) => sl.IsFoggy = b, sl => sl?.IsFoggy);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsFreezing)}", (sl, b) => sl.IsFreezing = b, sl => sl?.IsFreezing);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsHail)}", (sl, b) => sl.IsHail = b, sl => sl?.IsHail);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsSleet)}", (sl, b) => sl.IsSleet = b, sl => sl?.IsSleet);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsRain)}", (sl, b) => sl.IsRain = b, sl => sl?.IsRain);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsWarning)}", (sl, b) => sl.IsWarning = b, sl => sl?.IsWarning);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsHurricane)}", (sl, b) => sl.IsHurricane = b, sl => sl?.IsHurricane);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.IsTornado)}", (sl, b) => sl.IsTornado = b, sl => sl?.IsTornado);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.StateConditions)}", (sl, b) => sl.StateConditions = b, sl => sl?.StateConditions);
                SetElement(config, cond, wc, $"{nameof(DataLine.WeatherConditions)}.{nameof(WeatherConditions.Latitude)}", (sl, b) => sl.Latitude = b, sl => sl?.Latitude);
                historyData.AddLine(baseLine);

                lines.Add(baseLine);
            }

            if (_historyDataCache.ContainsKey(location))
            {
                _historyDataCache[location] = historyData;
            } else
            {
                _historyDataCache.Add(location, historyData);
            }
            OnHistoryDataUpdated(location);
            return historyData;
        }

        public async Task<SunData?> GetSunDataAsync(Location location)
        {
            var data = _sunDataCache.FirstOrDefault(_sunDataCache => _sunDataCache.Key.Equals(location)).Value;
            var config = ConfigManager.Config.SunData;
            if (IsDataStillValid(data, config))
                return data;

            var servicesToQuery = config.Elements
                .SelectMany(element => element.ServiceElements)
                .Select(se => se.ServiceName)
                .Distinct();
            var serviceQueries = new Dictionary<string, SunData>();
            if (!servicesToQuery.Any())
            {
                throw new InvalidOperationException("No services to query for SunData mentioned in the Elements. Please check the configuration.");
            }

            var sunData = config.OverlayExistingData ? (data ?? new SunData(DateTimeOffset.Now)) : new SunData(DateTimeOffset.Now);

            var datesToQuery = new List<DateTimeOffset>();
            var datesToHave = new List<DateTimeOffset>();
            for (var d = DateTimeOffset.Now.AddDays(-7); d <= DateTimeOffset.Now.AddDays(7); d = d.AddDays(1))
            {
                datesToHave.Add(d);
                if (sunData.Lines.Any(sunData => sunData.For.Date == d.Date))
                    continue;
                datesToQuery.Add(d);
            }
            if (!datesToQuery.Any() && data != null)
            {
                data.Pulled = DateTimeOffset.Now;
                return data;
            } 
            else if (!datesToQuery.Any())
            {
                throw new InvalidOperationException("There is some kind of issue where SunData did not have any lines to pull.");
            }
            var minDate = datesToQuery.Min();
            var maxDate = datesToQuery.Max();

            foreach (var s in servicesToQuery)
            {
                var service = SunDataServices.FirstOrDefault(q => string.Equals(q.ServiceName, s, StringComparison.OrdinalIgnoreCase));
                if (service == null)
                    throw new InvalidOperationException($"Service {s} does not exist, which was mentioned in CurrentData Elements.");

                var returnData = await service.GetSunDataAsync(location, minDate.Date, maxDate.Date);
                if (returnData != null)
                {
                    serviceQueries.Add(s, returnData);
                }
            }
            if (!serviceQueries.Any())
            {
                throw new InvalidOperationException($"No data was retrieved from any services. Check to make sure your API keys are correct!");
            }

            sunData.Sources = serviceQueries.Select(sq => sq.Value.Sources).SelectMany(s => s).Union(sunData.Sources).Distinct().ToArray();

            var lines = new List<SunLine>();
            foreach (var d in datesToHave)
            {
                var baseLine = sunData.Lines.FirstOrDefault(l => l.For.Date == d.Date);
                bool isNew = false;
                if (baseLine == null)
                {
                    baseLine = new SunLine(DateTimeOffset.Now) { For = d.Date, MoonData = new MoonData(DateTimeOffset.Now) { For = d.Date } };
                    isNew = true;
                }
                var individualLines = serviceQueries
                    .Where(sq => sq.Value.Lines.Any(l => l.For.Date == d.Date))
                    .ToDictionary(
                        sq => sq.Key,
                        sq => sq.Value.Lines.First(l => l.For.Date == d.Date)
                    );
                var moonLines = individualLines
                    .Where(il => il.Value.MoonData != null)
                    .ToDictionary(
                        il => il.Key,
                        il => (MoonData)il.Value.MoonData!
                    );
                if (isNew && !individualLines.Any()) continue;

                SetElement(config, baseLine, individualLines, nameof(SunLine.AstronomicalTwilightBegin), (sl, b) => sl.AstronomicalTwilightBegin = b, sl => sl.AstronomicalTwilightBegin);
                SetElement(config, baseLine, individualLines, nameof(SunLine.AstronomicalTwilightEnd), (sl, b) => sl.AstronomicalTwilightEnd = b, sl => sl.AstronomicalTwilightEnd);
                SetElement(config, baseLine, individualLines, nameof(SunLine.CivilTwilightBegin), (sl, b) => sl.CivilTwilightBegin = b, sl => sl.CivilTwilightBegin);
                SetElement(config, baseLine, individualLines, nameof(SunLine.CivilTwilightEnd), (sl, b) => sl.CivilTwilightEnd = b, sl => sl.CivilTwilightEnd);
                SetElement(config, baseLine, individualLines, nameof(SunLine.NauticalTwilightBegin), (sl, b) => sl.NauticalTwilightBegin = b, sl => sl.NauticalTwilightBegin);
                SetElement(config, baseLine, individualLines, nameof(SunLine.NauticalTwilightEnd), (sl, b) => sl.NauticalTwilightEnd = b, sl => sl.NauticalTwilightEnd);
                SetElement(config, baseLine, individualLines, nameof(SunLine.Sunrise), (sl, b) => sl.Sunrise = b, sl => sl.Sunrise);
                SetElement(config, baseLine, individualLines, nameof(SunLine.Sunset), (sl, b) => sl.Sunset = b, sl => sl.Sunset);
                SetElement(config, baseLine, individualLines, nameof(SunLine.DayStart), (sl, b) => sl.DayStart = b, sl => sl.DayStart);
                SetElement(config, baseLine, individualLines, nameof(SunLine.DayEnd), (sl, b) => sl.DayEnd = b, sl => sl.DayEnd);
                SetElement(config, baseLine, individualLines, nameof(SunLine.SolarNoon), (sl, b) => sl.SolarNoon = b, sl => sl.SolarNoon);
                SetElement(config, baseLine, individualLines, nameof(SunLine.SunAzimuth), (sl, b) => sl.SunAzimuth = b, sl => sl.SunAzimuth);

                var moonData = baseLine.MoonData ?? new MoonData(DateTimeOffset.Now);
                baseLine.MoonData = moonData;
                SetElement(config, moonData, moonLines, $"{nameof(SunLine.MoonData)}.{nameof(MoonData.Latitude)}", (sl, b) => sl.Latitude = b, sl => sl.Latitude);
                SetElement(config, moonData, moonLines, $"{nameof(SunLine.MoonData)}.{nameof(MoonData.Longitude)}", (sl, b) => sl.Longitude = b, sl => sl.Longitude);
                SetElement(config, moonData, moonLines, $"{nameof(SunLine.MoonData)}.{nameof(MoonData.MoonDeclination)}", (sl, b) => sl.MoonDeclination = b, sl => sl.MoonDeclination);
                SetElement(config, moonData, moonLines, $"{nameof(SunLine.MoonData)}.{nameof(MoonData.MoonPhase)}", (sl, b) => sl.MoonPhase = b, sl => sl.MoonPhase);
                SetElement(config, moonData, moonLines, $"{nameof(SunLine.MoonData)}.{nameof(MoonData.MoonAzimuth)}", (sl, b) => sl.MoonAzimuth = b, sl => sl.MoonAzimuth);
                SetElement(config, moonData, moonLines, $"{nameof(SunLine.MoonData)}.{nameof(MoonData.MoonParallacticAngle)}", (sl, b) => sl.MoonParallacticAngle = b, sl => sl.MoonParallacticAngle);
                SetElement(config, moonData, moonLines, $"{nameof(SunLine.MoonData)}.{nameof(MoonData.MoonDistance)}", (sl, b) => sl.MoonDistance = b, sl => sl.MoonDistance);
                SetElement(config, moonData, moonLines, $"{nameof(SunLine.MoonData)}.{nameof(MoonData.MoonAltitude)}", (sl, b) => sl.MoonAltitude = b, sl => sl.MoonAltitude);

                lines.Add(baseLine);
            }
            sunData.Clear();
            sunData.AddLines(lines);

            if (_sunDataCache.ContainsKey(location))
            {
                _sunDataCache[location] = sunData;
            }
            else
            {
                _sunDataCache.Add(location, sunData);
            }
            OnSunDataUpdated(location);
            return sunData;
        }

        public async Task<AlertData?> GetAlertDataAsync(Location location)
        {
            var data = _alertDataCache.FirstOrDefault(_alertDataCache => _alertDataCache.Key.Equals(location)).Value;
            var config = ConfigManager.Config.AlertData;
            if (IsDataStillValid(data, config))
                return data;

            var servicesToQuery = config.Elements
                .SelectMany(element => element.ServiceElements)
                .Select(se => se.ServiceName)
                .Distinct();
            var serviceQueries = new Dictionary<string, AlertData>();
            if (!servicesToQuery.Any())
            {
                throw new InvalidOperationException("No services to query for AlertData mentioned in the Elements. Please check the configuration.");
            }

            var alertData = config.OverlayExistingData ? (data ?? new AlertData(DateTimeOffset.Now)) : new AlertData(DateTimeOffset.Now);
            alertData.Pulled = DateTimeOffset.Now;
            foreach (var s in servicesToQuery)
            {
                var service = AlertQueryServices.FirstOrDefault(q => string.Equals(q.ServiceName, s, StringComparison.OrdinalIgnoreCase));
                if (service == null)
                    throw new InvalidOperationException($"Service {s} does not exist, which was mentioned in AlertData Elements.");

                var returnData = await service.GetAlertDataAsync(location);
                if (returnData != null)
                {
                    serviceQueries.Add(s, returnData);
                }
            }
            if (!serviceQueries.Any())
            {
                throw new InvalidOperationException($"No data was retrieved from any services. Check to make sure your API keys are correct.");
            }
            foreach (var s in serviceQueries)
            {
                foreach (var l in s.Value.Alerts)
                {
                    alertData.AddLine(l);
                }
            }
            alertData.ClearExpiredAlerts();

            if (_alertDataCache.ContainsKey(location))
            {
                _alertDataCache[location] = alertData;
            } else
            {
                _alertDataCache.Add(location, alertData);
            }
            OnAlertDataUpdated(location);
            return alertData;
        }


        #region Helpers 

        private DateTimeOffset GetWeightedAverage(IEnumerable<Tuple<int, DateTimeOffset>> tuples)
        {
            var minDate = tuples.Min(t => t.Item2);
            double weightedTicks = 0;
            int totalWeight = 0;

            foreach (var t in tuples)
            {
                var deltaTicks = (t.Item2.UtcDateTime - minDate.UtcDateTime).Ticks;
                weightedTicks += t.Item1 * deltaTicks;
                totalWeight += t.Item1;
            }

            if (totalWeight > 0)
            {
                var avgDeltaTicks = weightedTicks / totalWeight;
                var result = minDate.UtcDateTime.AddTicks((long)avgDeltaTicks);
                return new DateTimeOffset(result.Ticks, minDate.Offset);
            }

            return DateTimeOffset.MinValue;
        }







        private float GetWeightedAverage(IEnumerable<Tuple<int, float>> tuples)
        {
            float weightedSum = 0;
            int totalWeight = 0;

            foreach (var t in tuples)
            {
                weightedSum += t.Item1 * t.Item2;
                totalWeight += t.Item1;
            }

            return totalWeight > 0 ? weightedSum / totalWeight : 0f;
        }

        private double GetWeightedAverage(IEnumerable<Tuple<int, double>> tuples)
        {
            double weightedSum = 0;
            int totalWeight = 0;

            foreach (var t in tuples)
            {
                weightedSum += t.Item1 * t.Item2;
                totalWeight += t.Item1;
            }

            return totalWeight > 0 ? weightedSum / totalWeight : 0f;
        }
        #endregion

        #region Set Elements

        private void SetElement<T>(DataConfig config, T currentData, Dictionary<string, T> serviceQueries, string elementName, Action<T, bool?> setValue, Func<T, bool?> getValue)
        {
            var elements = GetServiceElementConfigsForProperty(elementName, config);
            var strValue = getValue(currentData);
            foreach (var e in elements)
            {
                var serviceReturn = serviceQueries.FirstOrDefault(sq => sq.Key.Equals(e.ServiceName, StringComparison.OrdinalIgnoreCase));
                if (serviceReturn.Value != null)
                {
                    var retValue = getValue(serviceReturn.Value);
                    if (retValue != null)
                        strValue = retValue;
                }
            }
            setValue(currentData, strValue);
        }

        private void SetElement<T>(DataConfig config, T currentData, Dictionary<string, T> serviceQueries, string elementName, Action<T, MoonPhase?> setValue, Func<T, MoonPhase?> getValue)
        {
            var elements = GetServiceElementConfigsForProperty(elementName, config);
            var strValue = getValue(currentData);
            foreach (var e in elements)
            {
                var serviceReturn = serviceQueries.FirstOrDefault(sq => sq.Key.Equals(e.ServiceName, StringComparison.OrdinalIgnoreCase));
                if (serviceReturn.Value != null)
                {
                    var retValue = getValue(serviceReturn.Value);
                    if (retValue != null)
                        strValue = retValue;
                }
            }
            setValue(currentData, strValue);
        }

        private void SetElement<T>(DataConfig config, T currentData, Dictionary<string, T> serviceQueries, string elementName, Action<T, string?> setValue, Func<T, string?> getValue)
        {
            var elements = GetServiceElementConfigsForProperty(elementName, config);
            var strValue = getValue(currentData);
            foreach (var e in elements)
            {
                var serviceReturn = serviceQueries.FirstOrDefault(sq => sq.Key.Equals(e.ServiceName, StringComparison.OrdinalIgnoreCase));
                if (serviceReturn.Value != null)
                {
                    var retValue = getValue(serviceReturn.Value);
                    if (retValue != null)
                        strValue = retValue;
                }
            }
            setValue(currentData, strValue);
        }

        private void SetElement<T>(DataConfig config, T currentData, Dictionary<string, T> serviceQueries, string elementName, Action<T, float?> setValue, Func<T, float?> getValue)
        {
            var elements = GetServiceElementConfigsForProperty(elementName, config);
            var strValue = getValue(currentData);
            var averageValues = new List<Tuple<int, float>>();
            foreach (var e in elements)
            {
                var serviceReturn = serviceQueries.FirstOrDefault(sq => sq.Key.Equals(e.ServiceName, StringComparison.OrdinalIgnoreCase));
                if (serviceReturn.Value != null)
                {
                    var retValue = getValue(serviceReturn.Value);
                    if (retValue != null)
                    {
                        if (e.Action == "Average")
                            averageValues.Add(new Tuple<int, float>(e.Weight, retValue.Value));
                        else if (e.Action == "Override")
                        {
                            averageValues.Clear();
                            strValue = retValue;
                        }
                    }

                }
            }
            if (averageValues.Any())
            {
                strValue = GetWeightedAverage(averageValues);
            }
            setValue(currentData, strValue);
        }

        private void SetElement<T>(DataConfig config, T currentData, Dictionary<string, T> serviceQueries, string elementName, Action<T, DateTimeOffset> setValue, Func<T, DateTimeOffset> getValue)
        {
            var elements = GetServiceElementConfigsForProperty(elementName, config);
            var strValue = getValue(currentData);
            var averageValues = new List<Tuple<int, DateTimeOffset>>();
            foreach (var e in elements)
            {
                var serviceReturn = serviceQueries.FirstOrDefault(sq => sq.Key.Equals(e.ServiceName, StringComparison.OrdinalIgnoreCase));
                if (serviceReturn.Value != null)
                {
                    var retValue = getValue(serviceReturn.Value);
                    if (e.Action == "Average")
                        averageValues.Add(new Tuple<int, DateTimeOffset>(e.Weight, retValue));
                    else if (e.Action == "Override")
                    {
                        averageValues.Clear();
                        strValue = retValue;
                    }
                }
            }
            if (averageValues.Any())
            {
                strValue = GetWeightedAverage(averageValues);
            }
            setValue(currentData, strValue);
        }

        private void SetElement<T>(DataConfig config, T currentData, Dictionary<string, T> serviceQueries, string elementName, Action<T, double?> setValue, Func<T, double?> getValue)
        {
            var elements = GetServiceElementConfigsForProperty(elementName, config);
            var strValue = getValue(currentData);
            var averageValues = new List<Tuple<int, double>>();
            foreach (var e in elements)
            {
                var serviceReturn = serviceQueries.FirstOrDefault(sq => sq.Key.Equals(e.ServiceName, StringComparison.OrdinalIgnoreCase));
                if (serviceReturn.Value != null)
                {
                    var retValue = getValue(serviceReturn.Value);
                    if (retValue != null)
                    {
                        if (e.Action == "Average")
                            averageValues.Add(new Tuple<int, double>(e.Weight, retValue.Value));
                        else if (e.Action == "Override")
                        {
                            averageValues.Clear();
                            strValue = retValue;
                        }
                    }

                }
            }
            if (averageValues.Any())
            {
                strValue = GetWeightedAverage(averageValues);
            }
            setValue(currentData, strValue);
        }

        private void SetElement<T>(DataConfig config, T currentData, Dictionary<string, T> serviceQueries, string elementName, Action<T, WindDirection?> setValue, Func<T, WindDirection?> getValue)
        {
            var elements = GetServiceElementConfigsForProperty(elementName, config);
            var strValue = getValue(currentData);
            var averageValues = new List<Tuple<int, double>>();
            foreach (var e in elements)
            {
                var serviceReturn = serviceQueries.FirstOrDefault(sq => sq.Key.Equals(e.ServiceName, StringComparison.OrdinalIgnoreCase));
                if (serviceReturn.Value != null)
                {
                    var retValue = getValue(serviceReturn.Value);
                    if (retValue?.Direction != null)
                    {
                        if (e.Action == "Average")
                            averageValues.Add(new Tuple<int, double>(e.Weight, retValue.Direction.Value));
                        else if (e.Action == "Override")
                        {
                            averageValues.Clear();
                            strValue = retValue;
                        }
                    }

                }
            }
            if (averageValues.Any())
            {
                strValue = new WindDirection(GetWeightedAverage(averageValues));
            }
            setValue(currentData, strValue);
        }

        private void SetElement<T>(DataConfig config, T currentData, Dictionary<string, T> serviceQueries, string elementName, Action<T, Temperature?> setValue, Func<T, Temperature?> getValue)
        {
            var elements = GetServiceElementConfigsForProperty(elementName, config);
            var strValue = getValue(currentData);
            var averageValues = new List<Tuple<int, float>>();
            foreach (var e in elements)
            {
                var serviceReturn = serviceQueries.FirstOrDefault(sq => sq.Key.Equals(e.ServiceName, StringComparison.OrdinalIgnoreCase));
                if (serviceReturn.Value != null)
                {
                    var retValue = getValue(serviceReturn.Value);
                    if (retValue?.TemperatureValue != null)
                    {
                        if (e.Action == "Average")
                            if (retValue.TemperatureValue != null)
                                averageValues.Add(new Tuple<int, float>(e.Weight, retValue.To(TemperatureEnum.Fahrenheit)!.Value));
                        else if (e.Action == "Override")
                        {
                            averageValues.Clear();
                            strValue = retValue;
                        }
                    }

                }
            }
            if (averageValues.Any())
            {
                strValue = new Temperature((float?)GetWeightedAverage(averageValues));
            }
            setValue(currentData, strValue);
        }

        private void SetElement<T>(DataConfig config, T currentData, Dictionary<string, T> serviceQueries, string elementName, Action<T, MoonProperty?> setValue, Func<T, MoonProperty?> getValue)
        {
            var elements = GetServiceElementConfigsForProperty(elementName, config);
            var strValue = getValue(currentData);
            var averageValues = new List<Tuple<int, double>>();
            var averageDates = new List<Tuple<int, DateTimeOffset>>();
            foreach (var e in elements)
            {
                var serviceReturn = serviceQueries.FirstOrDefault(sq => sq.Key.Equals(e.ServiceName, StringComparison.OrdinalIgnoreCase));
                if (serviceReturn.Value != null)
                {
                    var retValue = getValue(serviceReturn.Value);
                    if (retValue != null)
                    {
                        if (e.Action == "Average")
                        {
                            averageValues.Add(new Tuple<int, double>(e.Weight, retValue.Value));
                            averageDates.Add(new Tuple<int, DateTimeOffset>(e.Weight, retValue.At));
                        }   
                        else if (e.Action == "Override")
                        {
                            averageValues.Clear();
                            strValue = retValue;
                        }
                    }

                }
            }
            if (averageValues.Any())
            {
                strValue = new MoonProperty(GetWeightedAverage(averageValues), GetWeightedAverage(averageDates));
            }
            setValue(currentData, strValue);
        }

        private void SetElement<T>(DataConfig config, T currentData, Dictionary<string, T> serviceQueries, string elementName, Action<T, Precipitation?> setValue, Func<T, Precipitation?> getValue)
        {
            var elements = GetServiceElementConfigsForProperty(elementName, config);
            var strValue = getValue(currentData);
            var averageValues = new List<Tuple<int, float>>();
            foreach (var e in elements)
            {
                var serviceReturn = serviceQueries.FirstOrDefault(sq => sq.Key.Equals(e.ServiceName, StringComparison.OrdinalIgnoreCase));
                if (serviceReturn.Value != null)
                {
                    var retValue = getValue(serviceReturn.Value);
                    if (retValue?.Amount != null)
                    {
                        if (e.Action == "Average")
                        {
                            var temp = retValue.To(PrecipitationEnum.Inches);
                            if (temp != null)
                                averageValues.Add(new Tuple<int, float>(e.Weight, temp.Value));
                        }

                        else if (e.Action == "Override")
                        {
                            averageValues.Clear();
                            strValue = retValue;
                        }
                    }

                }
            }
            if (averageValues.Any())
            {
                strValue = new Precipitation(GetWeightedAverage(averageValues), PrecipitationEnum.Inches);
            }
            setValue(currentData, strValue);
        }

        private void SetElement<T>(DataConfig config, T currentData, Dictionary<string, T> serviceQueries, string elementName, Action<T, Pressure?> setValue, Func<T, Pressure?> getValue)
        {
            var elements = GetServiceElementConfigsForProperty(elementName, config);
            var strValue = getValue(currentData);
            var averageValues = new List<Tuple<int, float>>();
            foreach (var e in elements)
            {
                var serviceReturn = serviceQueries.FirstOrDefault(sq => sq.Key.Equals(e.ServiceName, StringComparison.OrdinalIgnoreCase));
                if (serviceReturn.Value != null)
                {
                    var retValue = getValue(serviceReturn.Value);
                    if (retValue?.PressureValue != null)
                    {
                        if (e.Action == "Average")
                        {
                            var temp = retValue.To(PressureEnum.Millibars);
                            if (temp != null)
                                averageValues.Add(new Tuple<int, float>(e.Weight, temp.Value));
                        }

                        else if (e.Action == "Override")
                        {
                            averageValues.Clear();
                            strValue = retValue;
                        }
                    }

                }
            }
            if (averageValues.Any())
            {
                strValue = new Pressure(GetWeightedAverage(averageValues), PressureEnum.Millibars);
            }
            setValue(currentData, strValue);
        }

        private void SetElement<T>(DataConfig config, T currentData, Dictionary<string, T> serviceQueries, string elementName, Action<T, Distance?> setValue, Func<T, Distance?> getValue)
        {
            var elements = GetServiceElementConfigsForProperty(elementName, config);
            var strValue = getValue(currentData);
            var averageValues = new List<Tuple<int, float>>();
            foreach (var e in elements)
            {
                var serviceReturn = serviceQueries.FirstOrDefault(sq => sq.Key.Equals(e.ServiceName, StringComparison.OrdinalIgnoreCase));
                if (serviceReturn.Value != null)
                {
                    var retValue = getValue(serviceReturn.Value);
                    if (retValue?.DistanceValue != null)
                    {
                        if (e.Action == "Average")
                        {
                            var temp = retValue.To(DistanceEnum.Miles);
                            if (temp != null)
                                averageValues.Add(new Tuple<int, float>(e.Weight, temp.Value));
                        }

                        else if (e.Action == "Override")
                        {
                            averageValues.Clear();
                            strValue = retValue;
                        }
                    }

                }
            }
            if (averageValues.Any())
            {
                strValue = new Distance(GetWeightedAverage(averageValues), DistanceEnum.Miles);
            }
            setValue(currentData, strValue);
        }

        private void SetElement<T>(DataConfig config, T currentData, Dictionary<string, T> serviceQueries, string elementName, Action<T, WindSpeed?> setValue, Func<T, WindSpeed?> getValue)
        {
            var elements = GetServiceElementConfigsForProperty(elementName, config);
            var strValue = getValue(currentData);
            var averageValues = new List<Tuple<int, float>>();
            foreach (var e in elements)
            {
                var serviceReturn = serviceQueries.FirstOrDefault(sq => sq.Key.Equals(e.ServiceName, StringComparison.OrdinalIgnoreCase));
                if (serviceReturn.Value != null)
                {
                    var retValue = getValue(serviceReturn.Value);
                    if (retValue?.SpeedValue != null)
                    {
                        if (e.Action == "Average")
                        {
                            var temp = retValue.To(WindSpeedEnum.MilesPerHour);
                            if (temp != null)
                                averageValues.Add(new Tuple<int, float>(e.Weight, temp.Value));
                        }

                        else if (e.Action == "Override")
                        {
                            averageValues.Clear();
                            strValue = retValue;
                        }
                    }

                }
            }
            if (averageValues.Any())
            {
                strValue = new WindSpeed(GetWeightedAverage(averageValues), WindSpeedEnum.MilesPerHour);
            }
            setValue(currentData, strValue);
        }

        private void SetElement<T>(DataConfig config, T currentData, Dictionary<string, T> serviceQueries, string elementName, Action<T, Coordinates?> setValue, Func<T, Coordinates?> getValue)
        {
            var elements = GetServiceElementConfigsForProperty(elementName, config);
            var strValue = getValue(currentData);
            var averageLatitude = new List<Tuple<int, double>>();
            var averageLongitude = new List<Tuple<int, double>>();
            foreach (var e in elements)
            {
                var serviceReturn = serviceQueries.FirstOrDefault(sq => sq.Key.Equals(e.ServiceName, StringComparison.OrdinalIgnoreCase));
                if (serviceReturn.Value != null)
                {
                    var retValue = getValue(serviceReturn.Value);
                    if (retValue?.Longitude != null && retValue?.Latitude != null)
                    {
                        if (e.Action == "Average")
                        {
                            averageLatitude.Add(new Tuple<int, double>(e.Weight, retValue.Latitude));
                            averageLongitude.Add(new Tuple<int, double>(e.Weight, retValue.Longitude));
                        }

                        else if (e.Action == "Override")
                        {
                            averageLatitude.Clear();
                            averageLongitude.Clear();
                            strValue = retValue;
                        }
                    }

                }
            }
            if (averageLatitude.Any() && averageLongitude.Any())
            {
                strValue = new Coordinates(GetWeightedAverage(averageLatitude), GetWeightedAverage(averageLongitude));  
            }
            setValue(currentData, strValue);
        }

        private void SetElement<T>(DataConfig config, T currentData, Dictionary<string, T> serviceQueries, string elementName, Action<T, DateTimeOffset?> setValue, Func<T, DateTimeOffset?> getValue)
        {
            var elements = GetServiceElementConfigsForProperty(elementName, config);
            var strValue = getValue(currentData);
            var averageTime = new List<Tuple<int, DateTimeOffset>>();
            foreach (var e in elements)
            {
                var serviceReturn = serviceQueries.FirstOrDefault(sq => sq.Key.Equals(e.ServiceName, StringComparison.OrdinalIgnoreCase));
                if (serviceReturn.Value != null)
                {
                    var retValue = getValue(serviceReturn.Value);
                    if (retValue != null)
                    {
                        if (e.Action == "Average")
                        {
                            averageTime.Add(new Tuple<int, DateTimeOffset>(e.Weight, retValue.Value));
                        }

                        else if (e.Action == "Override")
                        {
                            averageTime.Clear();
                            strValue = retValue;
                        }
                    }

                }
            }
            if (averageTime.Any())
            {
                strValue = GetWeightedAverage(averageTime);
            }
            setValue(currentData, strValue);
        }
        #endregion
    }
}
