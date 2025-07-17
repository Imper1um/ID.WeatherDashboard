using ID.WeatherDashboard.API.Codes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    public class Alert : IPulledData
    {
        public Alert(DateTimeOffset? pulled, params string[] sources)
        {
            if (pulled != null)
                Pulled = pulled.Value;

            Sources = sources;
        }

        public DateTimeOffset Pulled { get; set; } = DateTimeOffset.Now;
        public string[] Sources { get; set; }

        public string? Headline { get; set; }

        public AlertMessageTypeEnum? MessageType { get; set; }
        public AlertSeverityEnum? Severity { get; set; }
        public AlertUrgencyEnum? Urgency { get; set; }
        public string? Areas { get; set; }
        public AlertCategoryEnum? Category { get; set; }
        public AlertCertaintyEnum? Certainty { get; set; }
        public string? Event { get; set; }
        public string? Note { get; set; }
        public DateTimeOffset? Effective { get; set; }
        public DateTimeOffset? Expires { get; set; }
        public string? Description { get; set; }
        public string? Instruction { get; set; }
    }
}
