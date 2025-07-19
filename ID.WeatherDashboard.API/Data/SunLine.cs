namespace ID.WeatherDashboard.API.Data
{
    public class SunLine
    {
        public SunLine(DateTimeOffset pulled, params string[] sources)
        {
            Pulled = pulled;
            Sources = sources;
        }

        public DateTimeOffset Pulled { get; set; }
        public string[] Sources { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTimeOffset"> when night has ended (-18°).
        /// </summary>
        public DateTimeOffset? AstronomicalTwilightBegin { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="DateTimeOffset"/> when morning nautical twilight begins (-12°)."/>
        /// </summary>
        public DateTimeOffset? NauticalTwilightBegin { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="DateTimeOffset"/> when morning civil twilight begins (-6°).
        /// </summary>
        public DateTimeOffset? CivilTwilightBegin { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="DateTimeOffset"/> when the sun rises (+0°).
        /// </summary>
        public DateTimeOffset? Sunrise { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="DateTimeOffset"/> when the day starts (+6°)
        /// </summary>
        public DateTimeOffset? DayStart { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="DateTimeOffset"/> when solar noon occurs (when the sun is at its highest point in the sky).
        /// </summary>
        public DateTimeOffset? SolarNoon { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="DateTimeOffset"/> when the day ends (+6°).
        /// </summary>
        public DateTimeOffset? DayEnd { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="DateTimeOffset"/> when the sun sets (+0°).
        /// </summary>
        public DateTimeOffset? Sunset { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="DateTimeOffset"/> when evening civil twilight ends (-6°).
        /// </summary>
        public DateTimeOffset? CivilTwilightEnd { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="DateTimeOffset"/> when evening nautical twilight ends (-12°).
        /// </summary>
        public DateTimeOffset? NauticalTwilightEnd { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="DateTimeOffset"/> when night has begun (-18°).
        /// </summary>
        public DateTimeOffset? AstronomicalTwilightEnd { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MoonData"/> for this specific time.
        /// </summary>
        public MoonData? MoonData { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the location for which the sun data is calculated.
        /// </summary>
        public double? Latitude { get; set; }
        /// <summary>
        /// Gets or sets the sun azimuth (the angle of the sun measured clockwise from north) at the given time.
        /// </summary>
        public double? SunAzimuth { get; set; }

        public DateTime For { get; set; }
    }
}
