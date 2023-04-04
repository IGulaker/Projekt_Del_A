using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

using Assignment_A1_03.Models;
using Assignment_A1_03.Services;

namespace Assignment_A1_03
{
    class Program
    {
        static void Main(string[] args)
        {
            OpenWeatherService service = new OpenWeatherService();

            service.WeatherForecastAvailable += EventHandler_WeatherForecastAvailable;
            Task<Forecast>[] tasks = { null, null, null, null };
            Exception exception = null;
            try
            {
                double latitude = 59.5086798659495;
                double longitude = 18.2654625932976;

                tasks[0] = service.GetForecastAsync(latitude, longitude);
                tasks[1] = service.GetForecastAsync("Miami");

                Task.WaitAll(tasks[0], tasks[1]);

                tasks[2] = service.GetForecastAsync(latitude, longitude);
                tasks[3] = service.GetForecastAsync("Miami");

                Task.WaitAll(tasks[2], tasks[3]);
            }
            catch (Exception ex)
            {
                exception = ex;
                Console.WriteLine(
                    "City weather service error" +
                    Environment.NewLine +
                    exception);
            }

            foreach (var task in tasks)
            {
                if (task.IsFaulted)
                {
                    OutputTaskErrorMessage(task);
                    continue;
                }

                if (tasks.First().Equals(task))
                    WriteSeparator();

                Console.WriteLine($"Väderprognos för {task.Result.City}:");
                OutputForecastByDate(task.Result);
            }
        }

        private static void OutputForecastByDate(Forecast forecast)
        {
            var orderedForecasts = forecast.Items.GroupBy(fItem => fItem.DateTime.Date).ToList();

            orderedForecasts.ForEach(orderedF =>
            {
                Console.WriteLine(orderedF.Key.ToShortDateString());
                foreach (var forecastItem in orderedF)
                    Console.WriteLine($"\t- {forecastItem.DateTime.TimeOfDay}: {forecastItem.Description}, temperatur: {forecastItem.Temperature}, vind: {forecastItem.WindSpeed} m/s");
            });

            WriteSeparator();
        }

        private static void OutputTaskErrorMessage(Task<Forecast> task)
        {
            Console.WriteLine(
                "Task exectuion error:" +
                Environment.NewLine +
                task.Exception);
        }

        private static void WriteSeparator() =>
            Console.WriteLine("------------------------");

        static void EventHandler_WeatherForecastAvailable(object sender, string eMessage) =>
            Console.WriteLine(eMessage);
    }
}
