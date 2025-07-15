using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    public class SunData
    {
        public SunData(DateTimeOffset? pulled, params SunLine[] lines)
        {
            Pulled = pulled ?? DateTimeOffset.Now;
            AddLines(lines);
        }

        public DateTimeOffset Pulled { get; set; }

        private List<SunLine> _lines = new List<SunLine>();

        public IEnumerable<SunLine> Lines => _lines.ToList();

        public void AddLine(SunLine line)
        {
            if (line != null)
            {
                _lines.Add(line);
            }
        }

        public void AddLines(IEnumerable<SunLine> lines)
        {
            if (lines != null)
            {
                foreach (var line in lines)
                {
                    AddLine(line);
                }
            }
        }

        public void PruneOlderThan(DateTime date)
        {
            _lines.RemoveAll(l => l.For < date);
        }
    }
}
