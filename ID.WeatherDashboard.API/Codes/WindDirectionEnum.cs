namespace ID.WeatherDashboard.API.Codes
{
    /// <summary>
    /// Represents the 16-point compass rose wind directions.
    /// </summary>
    public enum WindDirectionEnum
    {
        North,
        North_NorthEast,
        NorthEast,
        East_NorthEast,
        East,
        East_SouthEast,
        SouthEast,
        South_SouthEast,
        South,
        South_SouthWest,
        SouthWest,
        West_SouthWest,
        West,
        West_NorthWest,
        NorthWest,
        North_NorthWest
    }

    public static class WindDirectionEnumExtensions
    {
        /// <summary>
        /// Gets the short (abbreviated) name for the wind direction (e.g., "N", "NNE").
        /// </summary>
        /// <param name="value">The <see cref="WindDirectionEnum"/> value.</param>
        /// <returns>The short name for the wind direction.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is not a valid <see cref="WindDirectionEnum"/>.</exception>
        public static string GetShortName(this WindDirectionEnum value)
        {
            return value switch
            {
                WindDirectionEnum.North => "N",
                WindDirectionEnum.North_NorthEast => "NNE",
                WindDirectionEnum.NorthEast => "NE",
                WindDirectionEnum.East_NorthEast => "ENE",
                WindDirectionEnum.East => "E",
                WindDirectionEnum.East_SouthEast => "ESE",
                WindDirectionEnum.SouthEast => "SE",
                WindDirectionEnum.South_SouthEast => "SSE",
                WindDirectionEnum.South => "S",
                WindDirectionEnum.South_SouthWest => "SSW",
                WindDirectionEnum.SouthWest => "SW",
                WindDirectionEnum.West_SouthWest => "WSW",
                WindDirectionEnum.West => "W",
                WindDirectionEnum.West_NorthWest => "WNW",
                WindDirectionEnum.NorthWest => "NW",
                WindDirectionEnum.North_NorthWest => "NNW",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
            };
        }

        /// <summary>
        /// Gets the long (full) name for the wind direction (e.g., "North-Northeast").
        /// </summary>
        /// <param name="value">The <see cref="WindDirectionEnum"/> value.</param>
        /// <returns>The long name for the wind direction.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is not a valid <see cref="WindDirectionEnum"/>.</exception>
        public static string GetLongName(this WindDirectionEnum value)
        {
            return value switch
            {
                WindDirectionEnum.North => "North",
                WindDirectionEnum.North_NorthEast => "North-Northeast",
                WindDirectionEnum.NorthEast => "Northeast",
                WindDirectionEnum.East_NorthEast => "East-Northeast",
                WindDirectionEnum.East => "East",
                WindDirectionEnum.East_SouthEast => "East-Southeast",
                WindDirectionEnum.SouthEast => "Southeast",
                WindDirectionEnum.South_SouthEast => "South-Southeast",
                WindDirectionEnum.South => "South",
                WindDirectionEnum.South_SouthWest => "South-Southwest",
                WindDirectionEnum.SouthWest => "Southwest",
                WindDirectionEnum.West_SouthWest => "West-Southwest",
                WindDirectionEnum.West => "West",
                WindDirectionEnum.West_NorthWest => "West-Northwest",
                WindDirectionEnum.NorthWest => "Northwest",
                WindDirectionEnum.North_NorthWest => "North-Northwest",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
            };
        }

        /// <summary>
        /// Gets the lower and upper degree bounds (in degrees from North, 0-360) for the wind direction.
        /// </summary>
        /// <param name="value">The <see cref="WindDirectionEnum"/> value.</param>
        /// <returns>A tuple containing the lower and upper degree bounds.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is not a valid <see cref="WindDirectionEnum"/>.</exception>
        public static (double Lower, double Upper) GetDegreeBounds(this WindDirectionEnum value)
        {
            return value switch
            {
                WindDirectionEnum.North => (348.75, 11.25),
                WindDirectionEnum.North_NorthEast => (11.25, 33.75),
                WindDirectionEnum.NorthEast => (33.75, 56.25),
                WindDirectionEnum.East_NorthEast => (56.25, 78.75),
                WindDirectionEnum.East => (78.75, 101.25),
                WindDirectionEnum.East_SouthEast => (101.25, 123.75),
                WindDirectionEnum.SouthEast => (123.75, 146.25),
                WindDirectionEnum.South_SouthEast => (146.25, 168.75),
                WindDirectionEnum.South => (168.75, 191.25),
                WindDirectionEnum.South_SouthWest => (191.25, 213.75),
                WindDirectionEnum.SouthWest => (213.75, 236.25),
                WindDirectionEnum.West_SouthWest => (236.25, 258.75),
                WindDirectionEnum.West => (258.75, 281.25),
                WindDirectionEnum.West_NorthWest => (281.25, 303.75),
                WindDirectionEnum.NorthWest => (303.75, 326.25),
                WindDirectionEnum.North_NorthWest => (326.25, 348.75),
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
            };
        }

        /// <summary>
        /// Gets the <see cref="WindDirectionEnum"/> value corresponding to the given degrees from North.
        /// </summary>
        /// <param name="degrees">Degrees from North (0-360).</param>
        /// <returns>The corresponding <see cref="WindDirectionEnum"/> value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the calculated index is not a valid <see cref="WindDirectionEnum"/>.</exception>
        public static WindDirectionEnum ToWindDirection(this double degrees)
        {
            degrees = (degrees % 360 + 360) % 360;
            int index = (int)Math.Round(degrees / 22.5) % 16;
            return index switch
            {
                0 => WindDirectionEnum.North,
                1 => WindDirectionEnum.North_NorthEast,
                2 => WindDirectionEnum.NorthEast,
                3 => WindDirectionEnum.East_NorthEast,
                4 => WindDirectionEnum.East,
                5 => WindDirectionEnum.East_SouthEast,
                6 => WindDirectionEnum.SouthEast,
                7 => WindDirectionEnum.South_SouthEast,
                8 => WindDirectionEnum.South,
                9 => WindDirectionEnum.South_SouthWest,
                10 => WindDirectionEnum.SouthWest,
                11 => WindDirectionEnum.West_SouthWest,
                12 => WindDirectionEnum.West,
                13 => WindDirectionEnum.West_NorthWest,
                14 => WindDirectionEnum.NorthWest,
                15 => WindDirectionEnum.North_NorthWest,
                _ => throw new ArgumentOutOfRangeException(nameof(degrees), degrees, null),
            };
        }
    }
}
