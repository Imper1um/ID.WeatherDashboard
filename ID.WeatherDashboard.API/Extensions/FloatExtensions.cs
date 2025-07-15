
using ID.WeatherDashboard.API.Codes;

public static class FloatExtensions
{
    public static float? Convert(this float? val, TemperatureEnum from, TemperatureEnum to)
    {
        if (!val.HasValue) return null;
        return from switch
        {
            TemperatureEnum.Celsius when to == TemperatureEnum.Fahrenheit => (val.Value * 9 / 5) + 32,
            TemperatureEnum.Fahrenheit when to == TemperatureEnum.Celsius => (val.Value - 32) * 5 / 9,
            _ => val.Value
        };
    }

    public static float? Convert(this float? val, PressureEnum from, PressureEnum to)
    {
        if (!val.HasValue) return null;
        return from switch
        {
            PressureEnum.Millibars when to == PressureEnum.Hectopascals => val.Value,
            PressureEnum.Millibars when to == PressureEnum.InchesOfMercury => val.Value / 33.8639f,
            PressureEnum.Millibars when to == PressureEnum.Atmospheres => val.Value / 1013.25f,
            PressureEnum.Hectopascals when to == PressureEnum.Millibars => val.Value,
            PressureEnum.Hectopascals when to == PressureEnum.InchesOfMercury => val.Value / 33.8639f,
            PressureEnum.Hectopascals when to == PressureEnum.Atmospheres => val.Value / 1013.25f,
            PressureEnum.InchesOfMercury when to == PressureEnum.Millibars => val.Value * 33.8639f,
            PressureEnum.InchesOfMercury when to == PressureEnum.Hectopascals => val.Value * 33.8639f,
            PressureEnum.InchesOfMercury when to == PressureEnum.Atmospheres => val.Value / 29.92f,
            PressureEnum.Atmospheres when to == PressureEnum.Millibars => val.Value * 1013.25f,
            PressureEnum.Atmospheres when to == PressureEnum.Hectopascals => val.Value * 1013.25f,
            PressureEnum.Atmospheres when to == PressureEnum.InchesOfMercury => val.Value * 29.92f,
            _ => val.Value
        };
    }

    public static float? Convert(this float? val, PrecipitationEnum from, PrecipitationEnum to)     {
        if (!val.HasValue) return null;
        return from switch
        {
            PrecipitationEnum.Inches when to == PrecipitationEnum.Millimeters => val.Value * 25.4f,
            PrecipitationEnum.Inches when to == PrecipitationEnum.Centimeters => val.Value * 2.54f,
            PrecipitationEnum.Inches when to == PrecipitationEnum.Feet => val.Value / 12f,
            PrecipitationEnum.Inches when to == PrecipitationEnum.LitersPerSquareMeter => val.Value * 25.4f / 1000f,
            PrecipitationEnum.Millimeters when to == PrecipitationEnum.Inches => val.Value / 25.4f,
            PrecipitationEnum.Millimeters when to == PrecipitationEnum.Centimeters => val.Value / 10f,  
            PrecipitationEnum.Millimeters when to == PrecipitationEnum.Feet => val.Value / 304.8f,
            PrecipitationEnum.Millimeters when to == PrecipitationEnum.LitersPerSquareMeter => val.Value / 1000f,
            PrecipitationEnum.Centimeters when to == PrecipitationEnum.Inches => val.Value / 2.54f,
            PrecipitationEnum.Centimeters when to == PrecipitationEnum.Millimeters => val.Value * 10f,
            PrecipitationEnum.Centimeters when to == PrecipitationEnum.Feet => val.Value / 30.48f,
            PrecipitationEnum.Centimeters when to == PrecipitationEnum.LitersPerSquareMeter => val.Value / 100f,
            PrecipitationEnum.Feet when to == PrecipitationEnum.Inches => val.Value * 12f,
            PrecipitationEnum.Feet when to == PrecipitationEnum.Millimeters => val.Value * 304.8f,
            PrecipitationEnum.Feet when to == PrecipitationEnum.Centimeters => val.Value * 30.48f,
            PrecipitationEnum.Feet when to == PrecipitationEnum.LitersPerSquareMeter => val.Value * 304.8f / 1000f,
            _ => val.Value
        };
    }

    public static float? Convert(this float? val, WindSpeedEnum from, WindSpeedEnum to)
    {
        if (!val.HasValue) return null;
        return from switch
        {
            WindSpeedEnum.MilesPerHour when to == WindSpeedEnum.KilometersPerHour => val.Value * 1.60934f,
            WindSpeedEnum.MilesPerHour when to == WindSpeedEnum.MetersPerSecond => val.Value * 0.44704f,
            WindSpeedEnum.MilesPerHour when to == WindSpeedEnum.MetersPerHour => val.Value * 1609.34f / 3600f,
            WindSpeedEnum.KilometersPerHour when to == WindSpeedEnum.MilesPerHour => val.Value / 1.60934f,
            WindSpeedEnum.KilometersPerHour when to == WindSpeedEnum.MetersPerSecond => val.Value / 3.6f,
            WindSpeedEnum.KilometersPerHour when to == WindSpeedEnum.MetersPerHour => val.Value * 1000f / 3600f,
            WindSpeedEnum.MetersPerSecond when to == WindSpeedEnum.MilesPerHour => val.Value * 2.23694f,
            WindSpeedEnum.MetersPerSecond when to == WindSpeedEnum.KilometersPerHour => val.Value * 3.6f,
            WindSpeedEnum.MetersPerSecond when to == WindSpeedEnum.MetersPerHour => val.Value * 3600f,
            _ => val.Value
        };
    }

    public static float? Convert(this float? val, DistanceEnum from, DistanceEnum to)     {
        if (!val.HasValue) return null;
        return from switch
        {
            DistanceEnum.Miles when to == DistanceEnum.Kilometers => val.Value * 1.60934f,
            DistanceEnum.Miles when to == DistanceEnum.Meters => val.Value * 1609.34f,
            DistanceEnum.Miles when to == DistanceEnum.Feet => val.Value * 5280f,
            DistanceEnum.Miles when to == DistanceEnum.NauticalMiles => val.Value * 0.868976f,
            DistanceEnum.Kilometers when to == DistanceEnum.Miles => val.Value / 1.60934f,
            DistanceEnum.Kilometers when to == DistanceEnum.Meters => val.Value * 1000f,
            DistanceEnum.Kilometers when to == DistanceEnum.Feet => val.Value * 3280.84f,
            DistanceEnum.Kilometers when to == DistanceEnum.NauticalMiles => val.Value * 0.539957f,
            DistanceEnum.Meters when to == DistanceEnum.Miles => val.Value / 1609.34f,
            DistanceEnum.Meters when to == DistanceEnum.Kilometers => val.Value / 1000f,
            DistanceEnum.Meters when to == DistanceEnum.Feet => val.Value * 3.28084f,
            DistanceEnum.Meters when to == DistanceEnum.NauticalMiles => val.Value / 1852f,
            DistanceEnum.Feet when to == DistanceEnum.Miles => val.Value / 5280f,   
            DistanceEnum.Feet when to == DistanceEnum.Kilometers => val.Value / 3280.84f,
            DistanceEnum.Feet when to == DistanceEnum.Meters => val.Value / 3.28084f,
            DistanceEnum.Feet when to == DistanceEnum.NauticalMiles => val.Value / 6076.12f,
            DistanceEnum.NauticalMiles when to == DistanceEnum.Miles => val.Value / 0.868976f,
            DistanceEnum.NauticalMiles when to == DistanceEnum.Kilometers => val.Value / 0.539957f,
            DistanceEnum.NauticalMiles when to == DistanceEnum.Meters => val.Value * 1852f,
            DistanceEnum.NauticalMiles when to == DistanceEnum.Feet => val.Value * 6076.12f,
            _ => val.Value
        };
    }
}

