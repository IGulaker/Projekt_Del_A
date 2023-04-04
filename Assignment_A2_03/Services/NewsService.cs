using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Assignment_A2_03.Models;
using Assignment_A2_03.ModelsSampleData;
using System.Net;

namespace Assignment_A2_03.Services
{
    public class NewsService
    {
        HttpClient httpClient = new HttpClient();
        ConcurrentDictionary<(NewsCategory, string), News> cachedNews = new();

        readonly string apiKey = "4fabf40756e6419bbd834751c06640a5";
        const string EventMessageBase = "Event-meddelande från weather service: ";
        const string NewDataMessageBase = " Nyheter i kategori finns tillgängliga: ";
        const string CachedDataMessageBase = "Cachelagrade nyheter i kategori finns tillgängliga: ";

        public event EventHandler<string> NewsAvailable;
        protected virtual void OnNewsAvailable(string message) =>
            NewsAvailable?.Invoke(this, message);

        public NewsService()
        {
            httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
            httpClient.DefaultRequestHeaders.Add("user-agent", "News-API-csharp/0.1");
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        public async Task<News> GetNewsAsync(NewsCategory category)
        {
            var cachedForecast = cachedNews.GetValueOrDefault((category, DateTime.Now.ToString("yyyy-MM-dd HH:mm")));
            if (cachedForecast is not null)
            {
                OnNewsAvailable(
                    EventMessageBase +
                    CachedDataMessageBase +
                    category.ToString() +
                    Environment.NewLine);
                return cachedForecast;
            }
            var uri = $"https://newsapi.org/v2/top-headlines?country=se&category={category}";

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            NewsApiData nd = await response.Content.ReadFromJsonAsync<NewsApiData>();

            var news = NewsApiDataToNews(nd);
            news.Category = category;

            cachedNews.TryAdd((category ,DateTime.Now.ToString("yyyy-MM-dd HH:mm")), news);
            OnNewsAvailable(
                EventMessageBase +
                NewDataMessageBase +
                category.ToString() +
                Environment.NewLine);
            return news;
        }

        private News NewsApiDataToNews(NewsApiData nData)
        {
            var news = new News()
            {
                Articles = nData.Articles.Select(item => new NewsItem()
                {
                    DateTime = item.PublishedAt,
                    Title = item.Title,
                    Description = item.Description,
                    Url = item.Url,
                    UrlToImage = $"{item.UrlToImage}"
                }).ToList()
            };

            return news;
        }
    }
}
