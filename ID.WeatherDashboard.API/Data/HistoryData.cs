using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    public class HistoryData
    {
        public HistoryData(params HistoryLine[] lines)
        {
            _lines.AddRange(lines);
        }

        private readonly List<HistoryLine> _lines = new List<HistoryLine>();
        public IEnumerable<HistoryLine> Lines => _lines.ToList();

        public void AddLine(HistoryLine line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            _lines.Add(line);
        }

        public void PruneOlderThan(DateTime date)
        {
            _lines.RemoveAll(l => l.Observed < date);
        }
    }
}
