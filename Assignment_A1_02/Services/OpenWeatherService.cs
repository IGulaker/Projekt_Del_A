using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

using Assignment_A1_02.Models;

namespace Assignment_A1_02.Services
{
    public class OpenWeatherService
    {
        HttpClient httpClient = new HttpClient();

        readonly string apiKey = "ef1d88b7c404add114633c5469439c95";
        const string BaseEventMessage = "Event-meddelande från weather service: Ny väderprognos för [LOCATION] tillgänglig";

        public event EventHandler<string> WeatherForecastAvailable;
        protected virtual void OnWeatherForecastAvailable (string message)
        {
            WeatherForecastAvailable?.Invoke(this, message);
        }
        public async Task<Forecast> GetForecastAsync(string City)
        {
            var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var uri = $"https://api.openweathermap.org/data/2.5/forecast?q={City}&units=metric&lang={language}&appid={apiKey}";

            Forecast forecast = await ReadWebApiAsync(uri);

            OnWeatherForecastAvailable(BaseEventMessage.Replace("[LOCATION]", forecast.City));
            return forecast;
        }
        public async Task<Forecast> GetForecastAsync(double latitude, double longitude)
        {
            var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var uri = $"https://api.openweathermap.org/data/2.5/forecast?lat={latitude}&lon={longitude}&units=metric&lang={language}&appid={apiKey}";

            Forecast forecast = await ReadWebApiAsync(uri);

            OnWeatherForecastAvailable(BaseEventMessage.Replace("[LOCATION]", $"({forecast.Latitude}, {forecast.Longitude})"));
            return forecast;
        }
        private async Task<Forecast> ReadWebApiAsync(string uri)
        {
            HttpResponseMessage response = await httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            WeatherApiData wd = await response.Content.ReadFromJsonAsync<WeatherApiData>();

            return WeatherApiDataToForecast(wd);
        }

        private Forecast WeatherApiDataToForecast(WeatherApiData wData)
        {
            var forecast = new Forecast()
            {
                City = wData.city.name,
                Latitude = wData.city.coord.lat,
                Longitude = wData.city.coord.lon,
                Items = wData.list.Select(wItem => new ForecastItem()
                {
                    DateTime = UnixTimeStampToDateTime(wItem.dt),
                    Temperature = wItem.main.temp,
                    WindSpeed = wItem.wind.speed,
                    Description = wItem.weather.FirstOrDefault().description
                }).ToList()
            };

            return forecast;
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}
