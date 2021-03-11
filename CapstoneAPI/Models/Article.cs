using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class Article
    {
        public int Id { get; set; }
        public string ArticleContent { get; set; }
        public DateTime PublishedDate { get; set; }
        public int Status { get; set; }
        public string UserId { get; set; }

        public virtual User User { get; set; }
    }
}
