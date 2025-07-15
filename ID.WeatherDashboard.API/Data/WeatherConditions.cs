using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    /// <summary>
    /// Represents various weather conditions at a specific point in time.
    /// </summary>
    public class WeatherConditions
    {
        public WeatherConditions(DateTimeOffset? time)
        {
            Time = time ?? DateTimeOffset.Now;
        }

        /// <summary>
        /// Gets or sets the time of the weather observation.
        /// </summary>
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// Gets or sets the rain rate
        /// </summary>
        public Precipitation? RainRate { get; set; }

        /// <summary>
        /// Gets or sets the snow rate
        /// </summary>
        public Precipitation? SnowRate { get; set; }

        /// <summary>
        /// Gets or sets the cloud cover percentage (0-100).
        /// </summary>
        public double? CloudCoverPercentage { get; set; }

        /// <summary>
        /// Gets or sets the moon phase.
        /// </summary>
        public MoonPhase? MoonPhase { get; set; }

        /// <summary>
        /// Gets or sets the wind gust speed
        /// </summary>
        public WindSpeed? WindGustSpeed { get; set; }

        /// <summary>
        /// Gets or sets the wind speed
        /// </summary>
        public WindSpeed? WindSpeed { get; set; }

        /// <summary>
        /// Gets or sets the visibility in miles.
        /// </summary>
        public Distance? Visibility { get; set; }

        /// <summary>
        /// Gets or sets the sun angle in degrees above the horizon.
        /// </summary>
        public double? SunAngle { get; set; }

        /// <summary>
        /// Gets or sets the moon angle in degrees above the horizon.
        /// </summary>
        public double? MoonAngle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether lightning is present.
        /// </summary>
        public bool? IsLightning { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether fog is present.
        /// </summary>
        public bool? IsFoggy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether freezing conditions are present.
        /// </summary>
        public bool? IsFreezing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether hail is present.
        /// </summary>
        public bool? IsHail { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether sleet is present.
        /// </summary>
        public bool? IsSleet { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether rain is present.
        /// </summary>
        public bool? IsRain { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a weather warning is in effect.
        /// </summary>
        public bool? IsWarning { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether hurricane conditions are present.
        /// </summary>
        public bool? IsHurricane { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether tornado conditions are present.
        /// </summary>
        public bool? IsTornado { get; set; }

        /// <summary>
        /// Gets or sets a string describing the state weather conditions.
        /// </summary>
        public string? StateConditions { get; set; }

        /// <summary>
        /// Creates a deep copy of this <see cref="WeatherConditions"/> instance.
        /// </summary>
        /// <returns>A new <see cref="WeatherConditions"/> instance with the same property values.</returns>
        public WeatherConditions Clone()
        {
            return new WeatherConditions(Time)
            {
                RainRate = RainRate,
                SnowRate = SnowRate,
                CloudCoverPercentage = CloudCoverPercentage,
                MoonPhase = MoonPhase,
                WindGustSpeed = WindGustSpeed,
                WindSpeed = WindSpeed,
                Visibility = Visibility,
                SunAngle = SunAngle,
                MoonAngle = MoonAngle,
                IsLightning = IsLightning,
                IsFoggy = IsFoggy,
                IsFreezing = IsFreezing,
                IsHail = IsHail,
                IsWarning = IsWarning,
                IsHurricane = IsHurricane,
                IsTornado = IsTornado,
                StateConditions = StateConditions
            };
        }
    }
}
