using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class EntryMark
    {
        public int Id { get; set; }
        public int MajorDetailId { get; set; }
        public int SubjectGroupId { get; set; }
        public int Year { get; set; }
        public double Mark { get; set; }

        public virtual MajorDetail MajorDetail { get; set; }
        public virtual SubjectGroup SubjectGroup { get; set; }
    }
}
