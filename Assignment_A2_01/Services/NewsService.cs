using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

using Assignment_A2_01.Models;
using Assignment_A2_01.ModelsSampleData;

namespace Assignment_A2_01.Services
{
    public class NewsService
    {
        HttpClient httpClient = new HttpClient();
 
        // Your API Key
        readonly string apiKey = "4fabf40756e6419bbd834751c06640a5";

        public NewsService()
        {
            httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
            httpClient.DefaultRequestHeaders.Add("user-agent", "News-API-csharp/0.1");
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        public async Task<NewsApiData> GetNewsAsync()
        {
            var uri = $"https://newsapi.org/v2/top-headlines?country=se&category=sports"; 

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            NewsApiData nd = await response.Content.ReadFromJsonAsync<NewsApiData>();         
            return nd;
        }
    }
}
