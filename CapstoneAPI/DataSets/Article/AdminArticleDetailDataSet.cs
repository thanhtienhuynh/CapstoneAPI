using System;
using System.Collections.Generic;

namespace CapstoneAPI.DataSets.Article
{
    public class AdminArticleDetailDataSet
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string PublishedPage { get; set; }
        public string RootUrl { get; set; }
        public DateTime? PostedDate { get; set; }
        public string ShortDescription { get; set; }
        public string PostImageUrl { get; set; }
        public int? Status { get; set; }
        public DateTime? PublicFromDate { get; set; }
        public DateTime? PublicToDate { get; set; }
        public DateTime? CrawlerDate { get; set; }
        public int? ImportantLevel { get; set; }
        public int? Censor { get; set; }
        public List<int> UniversityIds { get; set; }

    }
}
