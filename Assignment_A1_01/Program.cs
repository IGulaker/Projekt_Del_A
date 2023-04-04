using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

using Assignment_A1_01.Models;
using Assignment_A1_01.Services;

namespace Assignment_A1_01
{
    class Program
    {
        static async Task Main(string[] args)
        {
            double latitude = 59.5086798659495;
            double longitude = 18.2654625932976;

            Forecast forecast = await new OpenWeatherService().GetForecastAsync(latitude, longitude);

            Console.WriteLine($"Weather forecast for {forecast.City}");
            WriteForecastByDate(forecast);
        }

        private static void WriteForecastByDate(Forecast forecast)
        {
            var orderedForecasts = forecast.Items.GroupBy(fItem => fItem.DateTime.Date).ToList();

            orderedForecasts.ForEach(orderedF =>
            {
                Console.WriteLine(orderedF.Key.ToShortDateString());
                foreach (var forecastItem in orderedF)
                    Console.WriteLine($"\t- {forecastItem.DateTime.TimeOfDay}: {forecastItem.Description}, temperatur: {forecastItem.Temperature}, vind: {forecastItem.WindSpeed} m/s");
            });
        }
    }
}
