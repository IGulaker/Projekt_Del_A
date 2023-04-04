using System;
using System.Collections.Generic;

namespace Assignment_A1_03.Models
{
    public class Forecast
    {
        public string City { get; set; }
        public List<ForecastItem> Items { get; set; } = new List<ForecastItem>();
    }
}
