namespace ID.WeatherDashboard.API.Data
{
    public class MoonData : IPulledData
    {
        public MoonData(DateTimeOffset? pulled, params string[] sources)
        {
            if (pulled != null)
                Pulled = pulled.Value;
            Sources = sources;
        }

        public DateTimeOffset Pulled { get; set; } = DateTimeOffset.Now;
        public string[] Sources { get; set; } = Array.Empty<string>();
        public DateTime For { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTimeOffset? Moonrise { get; set; }
        public DateTimeOffset? Moonset { get; set; }
        public MoonPhase? MoonPhase { get; set; }

        public MoonProperty? MoonDeclination { get; set; }
        public MoonProperty? MoonAzimuth { get; set; }
        public MoonProperty? MoonParallacticAngle { get; set; }
        public MoonProperty? MoonDistance { get; set; }
        public MoonProperty? MoonAltitude { get; set; }

        public MoonProperty? CalculateDeclination()
        {
            if (Latitude == null || MoonAzimuth == null || MoonAltitude == null)
                return null;

            double latRad = Latitude.Value.ToRadians();
            double azimuthRad = MoonAzimuth.Value.ToRadians();
            double altitudeRad = MoonAltitude.Value.ToRadians();

            double declinationRad = Math.Asin(
                Math.Sin(latRad) * Math.Sin(altitudeRad) +
                Math.Cos(latRad) * Math.Cos(altitudeRad) * Math.Cos(azimuthRad)
            );

            double declinationDeg = declinationRad.ToDegrees();

            var averageTime = AverageDateTimes(MoonAzimuth.At, MoonAltitude.At);

            return new MoonProperty(declinationDeg, averageTime);
        }

        /// <summary>
        /// Approximates the Moon Angle at a given date and time based on the moon information provided. This is not exact, and it only works whenever the Date is on this specific time.
        /// </summary>
        /// <param name="date"><see cref="DateTimeOffset"/> to search for.</param>
        /// <returns>Angle of the moon (approximated) at the specific time.</returns>
        public float? MoonAngleAt(DateTimeOffset date)
        {
            if (Latitude == null || MoonDeclination == null || Moonrise == null || Moonset == null)
                return null;

            double latRad = Latitude.Value.ToRadians();
            double declinationRad = MoonDeclination.Value.ToRadians();

            var moonTransit = Moonrise.Value + (Moonset.Value - Moonrise.Value) / 2;
            var hoursFromTransit = (date - moonTransit).TotalHours;

            double hourAngleDeg = hoursFromTransit * 15.0;
            double hourAngleRad = hourAngleDeg.ToRadians();

            double sinAltitude = Math.Sin(latRad) * Math.Sin(declinationRad) +
                                 Math.Cos(latRad) * Math.Cos(declinationRad) * Math.Cos(hourAngleRad);

            double altitudeRad = Math.Asin(sinAltitude);

            return (float)altitudeRad.ToDegrees();
        }


        private DateTimeOffset AverageDateTimes(DateTimeOffset a, DateTimeOffset b) =>
            new DateTimeOffset((a.UtcDateTime.Ticks + b.UtcDateTime.Ticks) / 2, TimeSpan.Zero);
    }
}
