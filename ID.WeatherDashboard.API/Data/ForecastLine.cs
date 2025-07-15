namespace ID.WeatherDashboard.API.Data
{
    public class ForecastLine : DataLine
    {
        public ForecastLine(DateTimeOffset pulled, params string[] sources) : base(pulled, sources)
        {
        }

        public float? RainChance { get; set; }
        public float? SnowChance { get; set; }
    }
}
