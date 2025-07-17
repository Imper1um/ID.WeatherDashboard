using ID.WeatherDashboard.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.WUnderground.Data
{
    public class Observations
    {
        public DateTimeOffset Pulled { get; set; } = DateTimeOffset.Now;

        [JsonPropertyName("observations")]
        public List<Observation>? ObservationLines { get; set; }

        public CurrentData? ToCurrentData()
        {
            return ObservationLines?.FirstOrDefault()?.ToCurrentData();
        }

        public HistoryData? ToHistoryData()
        {
            if (ObservationLines?.Any() != true) return null;
            var lines = ObservationLines?.Select(o => o.ToHistoryLine())
                .Where(l => l != null);
            if (lines?.Any() != true) return null;

            return new HistoryData(Pulled, lines.ToArray())
            {
                Sources = new[] {"WUnderground"} 
            };
        }
    }
}
