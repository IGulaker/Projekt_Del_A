using System;
using System.Collections.Generic;
using System.Text;

namespace Assignment_A1_02.Models
{
    public class Forecast
    {
        public string City { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<ForecastItem> Items { get; set; } = new List<ForecastItem>();
    }
}
