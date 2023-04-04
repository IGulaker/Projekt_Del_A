using System.Collections.Generic;
using System.Xml.Serialization;

namespace Assignment_A2_03.Models
{
    public enum NewsCategory
    {
        entertainment, general, health, science, technology
    }
 
    [XmlRoot("News", Namespace = "http://mynamespace/test/")] //ths to be able to deserialize the sample data
    public class News
    {
        public NewsCategory Category { get; set; }
        public List<NewsItem> Articles { get; set; } = new List<NewsItem>();
    }
}
