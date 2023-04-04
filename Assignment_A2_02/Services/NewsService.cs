using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Assignment_A2_02.Models;
using Assignment_A2_02.ModelsSampleData;
namespace Assignment_A2_02.Services
{
    public class NewsService
    {
        HttpClient httpClient = new HttpClient();

        readonly string apiKey = "4fabf40756e6419bbd834751c06640a5";
        const string EventMessageBase = "Event-meddelande från weather service: Nyheter i kategori finns tillgängliga: ";

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
            var uri = $"https://newsapi.org/v2/top-headlines?country=se&category={category}";

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            NewsApiData nd = await response.Content.ReadFromJsonAsync<NewsApiData>();

            OnNewsAvailable(
                EventMessageBase + 
                category.ToString() +
                Environment.NewLine);
            return NewsApiDataToNews(nd, category);
        }

        private News NewsApiDataToNews(NewsApiData nData, NewsCategory category)
        {
            var news = new News()
            {
                Category = category,
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
