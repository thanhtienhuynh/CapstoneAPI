using CapstoneAPI.DataSets.University;
using System;
using System.Collections.Generic;

namespace CapstoneAPI.DataSets.Article
{
    public class ApprovingArticleResponse
    {
        public int Id { get; set; }
        public DateTime? PublicFromDate { get; set; }
        public DateTime? PublicToDate { get; set; }
        public int? Status { get; set; }
        public virtual ICollection<ApprovingArticleUniversityResponse> Universities { get; set; }
    }
}
