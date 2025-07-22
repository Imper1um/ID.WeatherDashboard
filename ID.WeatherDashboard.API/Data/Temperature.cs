using ID.WeatherDashboard.API.Codes;

namespace ID.WeatherDashboard.API.Data
{
    /// <summary>
    ///     Represents a temperature value stored in a specific unit.
    /// </summary>
    public class Temperature
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Temperature"/> class.
        /// </summary>
        /// <param name="temperature">The temperature value.</param>
        /// <param name="storedAs">The unit the value is stored in.</param>
        public Temperature(float? temperature, TemperatureEnum storedAs = TemperatureEnum.Fahrenheit)
        {
            TemperatureValue = temperature;
            StoredAs = storedAs;
        }

        /// <summary>
        ///     Gets the temperature value.
        /// </summary>
        public float? TemperatureValue { get; }

        /// <summary>
        ///     Gets the unit the value is stored in.
        /// </summary>
        public TemperatureEnum StoredAs { get; } = TemperatureEnum.Fahrenheit;

        /// <summary>
        ///     Converts the value to the specified unit.
        /// </summary>
        /// <param name="target">The target unit.</param>
        /// <returns>The converted temperature, or <see langword="null"/> if <see cref="TemperatureValue"/> is <see langword="null"/>.</returns>
        public float? To(TemperatureEnum target)
        {
            return TemperatureValue.Convert(StoredAs, target);
        }
    }
}
