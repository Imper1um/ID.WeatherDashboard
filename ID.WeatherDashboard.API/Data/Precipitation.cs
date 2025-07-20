using ID.WeatherDashboard.API.Codes;

namespace ID.WeatherDashboard.API.Data
{
    public class Precipitation
    {
        public Precipitation(float? amount, PrecipitationEnum storedAs = PrecipitationEnum.Millimeters)
        {
            Amount = amount;
            StoredAs = storedAs;
        }
        public float? Amount { get; }
        public PrecipitationEnum StoredAs { get; } = PrecipitationEnum.Millimeters;
        public float? To(PrecipitationEnum target)
        {
            return Amount.Convert(StoredAs, target);
        }
    }
}
