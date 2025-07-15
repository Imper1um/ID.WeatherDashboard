using ID.WeatherDashboard.API.Codes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    public class Distance
    {
        public Distance(float? distance, DistanceEnum storedAs = DistanceEnum.Miles)
        {
            DistanceValue = distance;
            StoredAs = storedAs;
        }

        public float? DistanceValue { get; }
        public DistanceEnum StoredAs { get; } = DistanceEnum.Miles;
        public float? To(DistanceEnum target)
        {
            return DistanceValue.Convert(StoredAs, target);
        }
    }
}
