using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class Article
    {
        public Article()
        {
            MajorArticles = new HashSet<MajorArticle>();
            UniversityArticles = new HashSet<UniversityArticle>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string HeaderConfig { get; set; }
        public string Content { get; set; }
        public string PublishedPage { get; set; }
        public string RootUrl { get; set; }
        public DateTime? PostedDate { get; set; }
        public DateTime? CrawlerDate { get; set; }
        public int? ImportantLevel { get; set; }
        public int? Censor { get; set; }
        public string ShortDescription { get; set; }
        public string PostImageUrl { get; set; }
        public DateTime? PublicFromDate { get; set; }
        public DateTime? PublicToDate { get; set; }
        public int Status { get; set; }

        public virtual ICollection<MajorArticle> MajorArticles { get; set; }
        public virtual ICollection<UniversityArticle> UniversityArticles { get; set; }
    }
}
