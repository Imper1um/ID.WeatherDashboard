using ID.WeatherDashboard.API.Codes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    public class AlertData : IPulledData
    {
        public AlertData(DateTimeOffset pulled, params Alert[] alerts)
        {
            Pulled = pulled;
            if (alerts?.Any() == true)
            {
                ReplaceAlerts(alerts);
            }
        }


        public DateTimeOffset Pulled { get; set; } = DateTimeOffset.Now;

        private readonly List<Alert> _alerts = new List<Alert>();
        public IEnumerable<Alert> Alerts => _alerts.ToList();

        public void ReplaceAlerts(IEnumerable<Alert> alerts)
        {
            if (alerts == null) throw new ArgumentNullException(nameof(alerts));
            _alerts.Clear();
            _alerts.AddRange(alerts);
        }

        public void AddLine(Alert alert)
        {
            if (alert == null) throw new ArgumentNullException(nameof(alert));

            bool IsMatch(Alert a) =>
                a.Event == alert.Event &&
                a.Areas == alert.Areas &&
                a.Category == alert.Category;

            switch (alert.MessageType)
            {
                case AlertMessageTypeEnum.Update:
                case AlertMessageTypeEnum.Alert:
                    var alertsToUpdate = _alerts.Where(IsMatch);
                    if (alertsToUpdate.Any()) 
                    {
                        foreach (var existingAlert in alertsToUpdate)
                        {
                            existingAlert.Headline = alert.Headline;
                            existingAlert.MessageType = alert.MessageType;
                            existingAlert.Severity = alert.Severity;
                            existingAlert.Urgency = alert.Urgency;
                            existingAlert.Certainty = alert.Certainty;
                            existingAlert.Note = alert.Note;
                            existingAlert.Effective = alert.Effective;
                            existingAlert.Expires = alert.Expires;
                            existingAlert.Description = alert.Description;
                            existingAlert.Instruction = alert.Instruction;
                            existingAlert.Pulled = alert.Pulled;
                            existingAlert.Areas = alert.Areas;
                            existingAlert.Event = alert.Event;
                            existingAlert.Category = alert.Category;
                            existingAlert.Pulled = alert.Pulled;
                            existingAlert.Description = alert.Description;
                            existingAlert.Instruction = alert.Instruction;
                        }
                    }
                    else
                    {
                        _alerts.Add(alert);
                    }
                    break;

                case AlertMessageTypeEnum.Cancel:
                    _alerts.RemoveAll(IsMatch);
                    break;

                default:
                    _alerts.Add(alert);
                    break;
            }
        }
        public void ClearExpiredAlerts()
        {
            _alerts.RemoveAll(a => a.Expires.HasValue && a.Expires.Value < DateTimeOffset.Now);
        }
    }
}
