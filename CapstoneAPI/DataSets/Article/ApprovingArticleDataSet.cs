using CapstoneAPI.Models;
using System;
using System.Collections.Generic;

namespace CapstoneAPI.DataSets.Article
{
    public class ApprovingArticleDataSet
    {
        public int Id { get; set; }
        public DateTime? PublicFromDate { get; set; }
        public DateTime? PublicToDate { get; set; }
        public int? Status { get; set; }
        public List<int> University { get; set; }
        public List<int> Major { get; set; }
    }
}
