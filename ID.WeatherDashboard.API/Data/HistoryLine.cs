using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    public class HistoryLine : DataLine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryLine"/> class with the specified pulled time and sources.
        /// </summary>
        /// <param name="pulled">The time the data was pulled.</param>
        /// <param name="sources">The sources of the data.</param>
        public HistoryLine(DateTimeOffset pulled, params string[] sources) : base(pulled, sources)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryLine"/> class from a <see cref="CurrentData"/> instance.
        /// </summary>
        /// <param name="current">The <see cref="CurrentData"/> instance to copy data from.</param>
        public HistoryLine(CurrentData current)
            : base(current.Pulled, current.Sources)
        {
            Observed = current.Observed;
            StationId = current.StationId;
            WindDirection = current.WindDirection;
            Humidity = current.Humidity;
            CurrentTemperature = current.CurrentTemperature;
            FeelsLike = current.FeelsLike;
            HeatIndex = current.HeatIndex;
            DewPoint = current.DewPoint;
            UVIndex = current.UVIndex;
            Pressure = current.Pressure;
            Coordinates = current.Coordinates;
            WeatherConditions = current.WeatherConditions?.Clone();
        }

        public Temperature? High { get; set; }
        public Temperature? Low { get; set; }
    }
}
