using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.Article
{
    public class ArticleDetailDataSet
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
        public string HeaderConfig { get; set; }

    }
}
