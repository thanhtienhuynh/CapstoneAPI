using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class SubjectGroup
    {
        public SubjectGroup()
        {
            MajorSubjectGroups = new HashSet<MajorSubjectGroup>();
            SubjectGroupDetails = new HashSet<SubjectGroupDetail>();
        }

        public int Id { get; set; }
        public string GroupCode { get; set; }
        public int Status { get; set; }

        public virtual ICollection<MajorSubjectGroup> MajorSubjectGroups { get; set; }
        public virtual ICollection<SubjectGroupDetail> SubjectGroupDetails { get; set; }
    }
}
