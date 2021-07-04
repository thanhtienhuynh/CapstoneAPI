using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class MajorSubjectGroup
    {
        public MajorSubjectGroup()
        {
            EntryMarks = new HashSet<EntryMark>();
            SubjectWeights = new HashSet<SubjectWeight>();
        }

        public int Id { get; set; }
        public int MajorId { get; set; }
        public int SubjectGroupId { get; set; }
        public int Status { get; set; }

        public virtual Major Major { get; set; }
        public virtual SubjectGroup SubjectGroup { get; set; }
        public virtual ICollection<EntryMark> EntryMarks { get; set; }
        public virtual ICollection<SubjectWeight> SubjectWeights { get; set; }
    }
}
