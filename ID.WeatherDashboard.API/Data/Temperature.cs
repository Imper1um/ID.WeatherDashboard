using ID.WeatherDashboard.API.Codes;

namespace ID.WeatherDashboard.API.Data
{
    public class Temperature
    {
        public Temperature(float? temperature, TemperatureEnum storedAs = TemperatureEnum.Fahrenheit)
        {
            TemperatureValue = temperature;
            StoredAs = storedAs;
        }

        public float? TemperatureValue { get; }
        public TemperatureEnum StoredAs { get; } = TemperatureEnum.Fahrenheit;

        public float? To(TemperatureEnum target)
        {
            return TemperatureValue.Convert(StoredAs, target);
        }
    }
}
