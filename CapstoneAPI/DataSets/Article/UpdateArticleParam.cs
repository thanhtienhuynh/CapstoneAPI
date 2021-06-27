using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.Article
{
    public class UpdateArticleParam
    {
        public IFormFile PostImage { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ShortDescription { get; set; }
        public string PostImageUrl { get; set; }
        public List<int> UniversityIds { get; set; }
        public List<int> MajorIds { get; set; }
    }
}
