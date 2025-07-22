using ID.WeatherDashboard.API.Codes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    /// <summary>
    ///     Represents a weather alert pulled from an external service.
    /// </summary>
    public class Alert : IPulledData
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Alert"/> class.
        /// </summary>
        /// <param name="pulled">The time the alert data was pulled.</param>
        /// <param name="sources">A collection of service names that provided the data.</param>
        public Alert(DateTimeOffset? pulled, params string[] sources)
        {
            if (pulled != null)
            {
                Pulled = pulled.Value;
            }

            Sources = sources;
        }

        /// <summary>
        ///     Gets or sets the time the data was pulled.
        /// </summary>
        public DateTimeOffset Pulled { get; set; } = DateTimeOffset.Now;

        /// <summary>
        ///     Gets or sets the sources that produced this alert.
        /// </summary>
        public string[] Sources { get; set; }

        /// <summary>
        ///     Gets or sets the headline of the alert.
        /// </summary>
        public string? Headline { get; set; }

        /// <summary>
        ///     Gets or sets the alert message type.
        /// </summary>
        public AlertMessageTypeEnum? MessageType { get; set; }

        /// <summary>
        ///     Gets or sets the severity of the alert.
        /// </summary>
        public AlertSeverityEnum? Severity { get; set; }

        /// <summary>
        ///     Gets or sets the urgency of the alert.
        /// </summary>
        public AlertUrgencyEnum? Urgency { get; set; }

        /// <summary>
        ///     Gets or sets the affected areas.
        /// </summary>
        public string? Areas { get; set; }

        /// <summary>
        ///     Gets or sets the alert category.
        /// </summary>
        public AlertCategoryEnum? Category { get; set; }

        /// <summary>
        ///     Gets or sets the certainty level of the alert.
        /// </summary>
        public AlertCertaintyEnum? Certainty { get; set; }

        /// <summary>
        ///     Gets or sets the event description.
        /// </summary>
        public string? Event { get; set; }

        /// <summary>
        ///     Gets or sets additional notes about the alert.
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        ///     Gets or sets when the alert becomes effective.
        /// </summary>
        public DateTimeOffset? Effective { get; set; }

        /// <summary>
        ///     Gets or sets when the alert expires.
        /// </summary>
        public DateTimeOffset? Expires { get; set; }

        /// <summary>
        ///     Gets or sets the alert description text.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        ///     Gets or sets the recommended instructions.
        /// </summary>
        public string? Instruction { get; set; }
    }
}
