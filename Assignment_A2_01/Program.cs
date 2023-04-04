using System;

using Assignment_A2_01.Models;
using Assignment_A2_01.Services;

namespace Assignment_A2_01
{
    class Program
    {
        static async Task Main(string[] args)
        {
            NewsService _service = new NewsService();

            var newsData = await _service.GetNewsAsync();
            OutputNewsData(newsData);
        }

        static void OutputNewsData(NewsApiData newsData)
        {
            var orderedNews = newsData.Articles.GroupBy(nItem => nItem.Author).ToList();
            Console.WriteLine("Topp Artiklar:" + 
                Environment.NewLine);
            orderedNews.ForEach(orderedN =>
            {
                Console.WriteLine(orderedN.Key);
                foreach (var newsItem in orderedN)
                    Console.WriteLine($"\t" +
                        $"- {newsItem.Title.Replace($"- {orderedN.Key}", "")}"
                        +Environment.NewLine);
            });
        }
    }
}
