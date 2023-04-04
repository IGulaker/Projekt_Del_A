using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Text.Json;

using Assignment_A1_03.Models;

namespace Assignment_A1_03.Services
{
    public class OpenWeatherService
    {
        HttpClient httpClient = new HttpClient();

        ConcurrentDictionary<(double, double, string), Forecast> cachedGeoForecasts = new ConcurrentDictionary<(double, double, string), Forecast>();
        ConcurrentDictionary<(string, string), Forecast> cachedCityForecasts = new ConcurrentDictionary<(string, string), Forecast>();

        readonly string apiKey = "ef1d88b7c404add114633c5469439c95";
        const string EventMessageBase = "Event-meddelande från weather service: ";
        const string NewForecastMessageBase = "Ny väderprognos för [LOCATION] tillgänglig";
        const string CachedForecastMessageBase = "Cachelagrad väderprognos för [LOCATION] tillgänglig";

        public event EventHandler<string> WeatherForecastAvailable;
        protected virtual void OnWeatherForecastAvailable(string message)
        {
            WeatherForecastAvailable?.Invoke(this, message);
        }

        public async Task<Forecast> GetForecastAsync(string City)
        {
            var cachedForecast = cachedCityForecasts.GetValueOrDefault((City, DateTime.Now.ToString("yyyy-MM-dd HH:mm")));
            if (cachedForecast is not null)
            {
                OnWeatherForecastAvailable(EventMessageBase + CachedForecastMessageBase.Replace("[LOCATION]", City));
                return cachedForecast;
            }

            var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var uri = $"https://api.openweathermap.org/data/2.5/forecast?q={City}&units=metric&lang={language}&appid={apiKey}";

            Forecast forecast = await ReadWebApiAsync(uri);
            cachedCityForecasts.TryAdd((City, DateTime.Now.ToString("yyyy-MM-dd HH:mm")), forecast);

            OnWeatherForecastAvailable(EventMessageBase + NewForecastMessageBase.Replace("[LOCATION]", City));
            return forecast;

        }

        public async Task<Forecast> GetForecastAsync(double latitude, double longitude)
        {
            var cachedForecast = cachedGeoForecasts.GetValueOrDefault((latitude, longitude, DateTime.Now.ToString("yyyy-MM-dd HH:mm")));
            if (cachedForecast is not null)
            {
                OnWeatherForecastAvailable(EventMessageBase + CachedForecastMessageBase.Replace("[LOCATION]", $"({latitude}, {longitude})"));
                return cachedForecast;
            }
            var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var uri = $"https://api.openweathermap.org/data/2.5/forecast?lat={latitude}&lon={longitude}&units=metric&lang={language}&appid={apiKey}";

            Forecast forecast = await ReadWebApiAsync(uri);
            cachedGeoForecasts.TryAdd((latitude, longitude, DateTime.Now.ToString("yyyy-MM-dd HH:mm")), forecast);

            OnWeatherForecastAvailable(EventMessageBase + NewForecastMessageBase.Replace("[LOCATION]", $"({latitude}, {longitude})"));
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
