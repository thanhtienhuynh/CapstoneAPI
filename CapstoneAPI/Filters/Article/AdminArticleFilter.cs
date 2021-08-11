using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Filters.Article
{
    public class AdminArticleFilter
    {
        public string Search { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime PublicFromDate { get; set; }
        public DateTime PublicToDate { get; set; }
        public int? ImportantLevel { get; set; }
        public string PublishedPage { get; set; }
        public int Status { get; set; }
        /*
         * -1: All
         * 0: New
         * 1: Approved
         * 2: Rejected
         * 3: Published
         * 4: Expired
         * 5: (Considered)
         */
        public int? Order { get; set; }
        /*
         * 0: Sort by DESC CrawlerDate
         * 1: Sort by ASC CrawlerDate
         * 2: Sort by ASC Title
         * 3: Sort by DESC Title
         * 4: Sort by ASC PostedDate
         * 5: Sort by DESC PostedDate
         * 6: Sort by ASC ImportantLevel
         * 7: Sort by DESC ImportantLevel
         */
    }
}
