using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Assignment_A2_04.Models;
using Assignment_A2_04.ModelsSampleData;
using System.Net;

namespace Assignment_A2_04.Services
{
    public class NewsService
    {
        HttpClient httpClient = new HttpClient();

        readonly string apiKey = "4fabf40756e6419bbd834751c06640a5";
        const string EventMessageBase = "Event-meddelande från weather service: ";
        const string NewDataMessageBase = " Nyheter i kategori finns tillgängliga: ";
        const string CachedDataMessageBase = "XML-Cachelagrade nyheter i kategori finns tillgängliga: ";

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
            NewsCacheKey NewsCacheKey = new(category, DateTime.Now);
            if (NewsCacheKey.CacheExist)
            {
                OnNewsAvailable(
                    EventMessageBase +
                    CachedDataMessageBase +
                    category.ToString() +
                    Environment.NewLine);
                return News.Deserialize(NewsCacheKey.FileName);
            }
            var uri = $"https://newsapi.org/v2/top-headlines?country=se&category={category}";

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            NewsApiData nd = await response.Content.ReadFromJsonAsync<NewsApiData>();

            var news = NewsApiDataToNews(nd);
            news.Category = category;

            News.Serialize(news, NewsCacheKey.FileName);
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
