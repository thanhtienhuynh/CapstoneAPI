﻿using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class UniversityArticle
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public int UniversityId { get; set; }

        public virtual Article Article { get; set; }
        public virtual University University { get; set; }
    }
}
