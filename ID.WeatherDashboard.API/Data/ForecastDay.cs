namespace ID.WeatherDashboard.API.Data
{
    public class ForecastDay : ForecastLine
    {
        public ForecastDay(DateTimeOffset pulled, params string[] sources) : base(pulled, sources)
        {
        }

        private readonly List<ForecastLine> _lines = new List<ForecastLine>();

        /// <summary>
        /// Gets the collection of <see cref="ForecastLine"/>.
        /// </summary>
        public IEnumerable<ForecastLine> Lines => _lines.ToList();

        /// <summary>
        /// Adds a new <see cref="ForecastLine"> to the collection.
        /// </summary>
        /// <param name="line">The <see cref="ForecastLine"/> to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="line"> is null.</exception>
        public void AddLine(ForecastLine line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            _lines.Add(line);
        }

        /// <summary>
        /// Replaces the entire collection of <see cref="ForecastLine"/> with the provided lines.
        /// </summary>
        /// <param name="lines"><see cref="IEnumerable{ForecastLine}"/> to replace lines with.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="lines"/> is null.</exception>
        public void ReplaceLines(IEnumerable<ForecastLine> lines)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            _lines.Clear();
            _lines.AddRange(lines);
        }


        public DaytimeData? Daytime { get; set; }
        public NighttimeData? Nighttime { get; set; }
        public MoonPhase? MoonPhase { get; set; }
    }
}
