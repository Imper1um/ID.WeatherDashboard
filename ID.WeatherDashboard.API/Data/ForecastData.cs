namespace ID.WeatherDashboard.API.Data
{
    public class ForecastData : IPulledData
    {
        public ForecastData(DateTimeOffset pulled, IEnumerable<ForecastDay>? lines = null)
        {
            Pulled = pulled;
            if (lines != null)
            {
                ReplaceLines(lines);
            }   
        }

        private readonly List<ForecastDay> _lines = new List<ForecastDay>();
        
        /// <summary>
        /// Gets the collection of <see cref="ForecastLine"/>.
        /// </summary>
        public IEnumerable<ForecastDay> Days => _lines.ToList();

        public DateTimeOffset Pulled { get; set; }

        public string[] Sources { get; set; } = Array.Empty<string>();


        /// <summary>
        /// Adds a new <see cref="ForecastDay"> to the collection.
        /// </summary>
        /// <param name="line">The <see cref="ForecastLine"/> to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="line"> is null.</exception>
        public void AddLine(ForecastDay line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            _lines.Add(line);
        }

        /// <summary>
        /// Replaces the entire collection of <see cref="ForecastDay"/> with the provided lines.
        /// </summary>
        /// <param name="lines"><see cref="IEnumerable{ForecastDay}"/> to replace lines with.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="lines"/> is null.</exception>
        public void ReplaceLines(IEnumerable<ForecastDay> lines)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            _lines.Clear();
            _lines.AddRange(lines);
        }
    }
}
