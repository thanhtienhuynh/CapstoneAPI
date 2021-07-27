using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Article.DataSet
{
    public class AdminArticleCollapseDataSet
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string PublishedPage { get; set; }
        public DateTime? PostedDate { get; set; }
        public string ShortDescription { get; set; }
        public string PostImageUrl { get; set; }
        public int? Status { get; set; }
        public int? Censor { get; set; }
        public DateTime? PublicFromDate { get; set; }
        public DateTime? PublicToDate { get; set; }
        public DateTime? CrawlerDate { get; set; }
        public int? ImportantLevel { get; set; }
    }

    public class HomeArticle
    {
        public int Type { get; set; }
        //1 Top
        //2 Today
        //3 Old
        public List<AdminArticleCollapseDataSet> Articles { get; set; }
    }
}
