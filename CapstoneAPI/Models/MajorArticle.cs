using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class MajorArticle
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public int MajorId { get; set; }

        public virtual Article Article { get; set; }
        public virtual Major Major { get; set; }
    }
}
