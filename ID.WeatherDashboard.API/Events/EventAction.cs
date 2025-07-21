namespace ID.WeatherDashboard.API.Events
{
    public class EventAction(Action<EventAction, EventTrigger> executeOnTrigger, params EventTrigger[] triggers)
    {
        public required Action<EventAction, EventTrigger> Execution { get; set; } = executeOnTrigger;
        public List<EventTrigger> Triggers { get; } = [.. triggers];
        public bool IsTriggered { get; set; }
    }
}
