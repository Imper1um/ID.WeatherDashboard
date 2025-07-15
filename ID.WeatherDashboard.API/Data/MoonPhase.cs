using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    /// <summary>
    /// Represents the phases of the moon.
    /// </summary>
    public enum MoonPhase
    {
        NewMoon,
        WaxingCrescent,
        FirstQuarter,
        WaxingGibbous,
        FullMoon,
        WaningGibbous,
        LastQuarter,
        WaningCrescent
    }

    /// <summary>
    /// Extension methods for the <see cref="MoonPhase"/> enum.
    /// </summary>
    public static class MoonPhaseExtensions
    {
        /// <summary>
        /// Gets a user-friendly string representation of the <see cref="MoonPhase"/> value.
        /// </summary>
        /// <param name="phase">The <see cref="MoonPhase"/> value.</param>
        /// <returns>A user-friendly string for the moon phase.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="phase"/> is not a valid <see cref="MoonPhase"/> value.</exception>
        public static string ToFriendlyString(this MoonPhase phase)
        {
            return phase switch
            {
                MoonPhase.NewMoon => "New Moon",
                MoonPhase.WaxingCrescent => "Waxing Crescent",
                MoonPhase.FirstQuarter => "First Quarter",
                MoonPhase.WaxingGibbous => "Waxing Gibbous",
                MoonPhase.FullMoon => "Full Moon",
                MoonPhase.WaningGibbous => "Waning Gibbous",
                MoonPhase.LastQuarter => "Last Quarter",
                MoonPhase.WaningCrescent => "Waning Crescent",
                _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, null)
            };
        }
    }
}
