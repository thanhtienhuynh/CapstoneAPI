using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class WeightNumber
    {
        public WeightNumber()
        {
            EntryMarks = new HashSet<EntryMark>();
        }

        public int Id { get; set; }
        public int SubjectGroupId { get; set; }
        public int MajorId { get; set; }
        public int? SubjectId { get; set; }
        public int? Weight { get; set; }

        public virtual Major Major { get; set; }
        public virtual Subject Subject { get; set; }
        public virtual SubjectGroup SubjectGroup { get; set; }
        public virtual ICollection<EntryMark> EntryMarks { get; set; }
    }
}
