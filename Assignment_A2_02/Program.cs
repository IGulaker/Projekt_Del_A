using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Assignment_A2_02.Models;
using Assignment_A2_02.Services;

namespace Assignment_A2_02
{

    class Program
    {
        private static NewsService _newsService = new();
        private static List<Task<News>> newsTasks = new();
        private static NewsCategory[] Categories = Enum.GetValues<NewsCategory>();
        static async Task Main(string[] args)
        {
            try
            {
                _newsService.NewsAvailable += EventHandler_NewsAvailable;

                for (int i = 0; i < Categories.Length; i++)
                    newsTasks.Add(_newsService.GetNewsAsync(Categories[i]));

                await Task.WhenAll(newsTasks);
                OutputAllNewsByCategoryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error i Main under exekvering:");
                Console.WriteLine(ex.Message);
            }
        }

        private static void OutputAllNewsByCategoryAsync()
        {
            while (newsTasks.Count > 0)
            {
                Task<News> executedNewsTask = newsTasks.First();

                if (executedNewsTask.IsFaulted)
                {
                    OutputTaskErrorMessage(executedNewsTask);
                    newsTasks.Remove(executedNewsTask);
                }

                OutputNewsInCategory(executedNewsTask.Result);
                newsTasks.Remove(executedNewsTask);
            }

        }

        private static void OutputNewsInCategory(News news)
        {
            Console.WriteLine($"Nyheter in kategori: {news.Category}");
            news.Articles.ForEach(article =>
            {
                Console.WriteLine($"\t- {article.DateTime}: {article.Title}");
            });

            WriteSeparator();
        }

        private static void WriteSeparator() =>
            Console.WriteLine("------------------------");

        private static void OutputTaskErrorMessage(Task task)
        {
            Console.WriteLine(
                "Error on task execution:" +
                Environment.NewLine +
                task.Exception);
        }

        private static void EventHandler_NewsAvailable(object sender, string eMessage) =>
            Console.WriteLine(eMessage);
    }
}
