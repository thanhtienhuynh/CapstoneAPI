using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class SubjectWeight
    {
        public int Id { get; set; }
        public int SubjectGroupDetailId { get; set; }
        public int Weight { get; set; }
        public int MajorSubjectGroupId { get; set; }

        public virtual MajorSubjectGroup MajorSubjectGroup { get; set; }
        public virtual SubjectGroupDetail SubjectGroupDetail { get; set; }
    }
}
