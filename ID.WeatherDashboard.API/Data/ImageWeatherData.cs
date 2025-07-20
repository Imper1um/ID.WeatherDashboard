using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.API.Data
{
    public class ImageWeatherData
    {
        [Range(0f,1f)]
        public float CloudCover { get; set; }
        [Range(0f,1f)]
        public float Rain { get; set; }
        [Range(0f,1f)]
        public float Fog { get; set; }
        [Range(0f, 1f)]
        public float Lightning { get; set; }
        [Range(0f, 1f)]
        public float Wind { get; set; }
        [Range(0f, 1f)]
        public float Extreme { get; set; }
        [Range(0f, 1f)]
        public float Snow { get; set; }
        public TimeSpan MinTimeOfDay { get; set; }
        public TimeSpan MaxTimeOfDay { get; set; }
        public required string Description { get; set; }
    }
}
