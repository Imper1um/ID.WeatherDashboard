using ID.WeatherDashboard.API.Codes;
using ID.WeatherDashboard.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.WeatherAPI.Data
{
    public class WeatherApiAlert
    {
        public DateTimeOffset Pulled { get; set; } = DateTimeOffset.Now;
        [JsonPropertyName("headline")]
        public string? Headline { get; set; }
        [JsonPropertyName("msgtype")]
        public string? MessageType { get; set; }
        [JsonPropertyName("severity")]
        public string? Severity { get; set; }
        [JsonPropertyName("urgency")]
        public string? Urgency { get; set; }
        [JsonPropertyName("certainty")]
        public string? Certainty { get; set; }
        [JsonPropertyName("effective")]
        public DateTimeOffset? Effective { get; set; }
        [JsonPropertyName("expires")]
        public DateTimeOffset? Expires { get; set; }
        [JsonPropertyName("areas")]
        public string? Areas { get; set; }
        [JsonPropertyName("category")]
        public string? Category { get; set; }
        [JsonPropertyName("event")]
        public string? Event { get; set; }
        [JsonPropertyName("note")]
        public string? Note { get; set; }
        [JsonPropertyName("desc")]
        public string? Description { get; set; }
        [JsonPropertyName("instruction")]
        public string? Instruction { get; set; }

        public Alert ToAlert()
        {
            return new Alert(Pulled, "WeatherAPI")
            {
                Headline = Headline,
                MessageType = MessageTypeEnum,
                Severity = SeverityEnum,
                Urgency = UrgencyEnum,
                Areas = Areas,
                Category = CategoryEnum,
                Certainty = CertaintyEnum,
                Event = Event,
                Note = Note,
                Effective = Effective,
                Expires = Expires,
                Description = Description,
                Instruction = Instruction
            };
        }

        public AlertMessageTypeEnum? MessageTypeEnum
        {
            get
            {
                if (Enum.TryParse<AlertMessageTypeEnum>(MessageType, out var mt))
                    return mt;
                return null;
            }
        }

        public AlertSeverityEnum? SeverityEnum
        {
            get
            {
                if (Enum.TryParse<AlertSeverityEnum>(Severity, out var s))
                    return s;
                return null;
            }
        }

        public AlertUrgencyEnum? UrgencyEnum
        {
            get
            {
                if (Enum.TryParse<AlertUrgencyEnum>(Urgency, out var u))
                    return u;
                return null;
            }
        }

        public AlertCertaintyEnum? CertaintyEnum
        {
            get
            {
                if (Enum.TryParse<AlertCertaintyEnum>(Certainty, out var c))
                    return c;
                return null;
            }
        }

        public AlertCategoryEnum? CategoryEnum
        {
            get
            {
                if (Enum.TryParse<AlertCategoryEnum>(Category, out var c))
                    return c;
                return null;
            }
        }
    }
}
