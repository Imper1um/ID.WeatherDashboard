using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    public class HistoryData : IPulledData
    {
        public HistoryData(DateTimeOffset? pulled, params HistoryLine[] lines)
        {
            _lines.AddRange(lines);
            Pulled = pulled ?? DateTimeOffset.Now;
        }

        public string[] Sources { get; set; } = Array.Empty<string>();

        public DateTimeOffset Pulled { get; set; } = DateTimeOffset.Now;

        private readonly List<HistoryLine> _lines = new List<HistoryLine>();
        public IEnumerable<HistoryLine> Lines => _lines.ToList();

        public void AddLine(HistoryLine line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            _lines.Add(line);
            Pulled = _lines.Max(l => l.Pulled);
        }

        public void PruneOlderThan(DateTime date)
        {
            _lines.RemoveAll(l => l.Observed < date);
        }
    }
}
