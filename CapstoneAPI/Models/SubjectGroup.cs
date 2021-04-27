using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class SubjectGroup
    {
        public SubjectGroup()
        {
            EntryMarks = new HashSet<EntryMark>();
            SubjectGroupDetails = new HashSet<SubjectGroupDetail>();
            UserMajorDetails = new HashSet<UserMajorDetail>();
            WeightNumbers = new HashSet<WeightNumber>();
        }

        public int Id { get; set; }
        public string GroupCode { get; set; }
        public int Status { get; set; }

        public virtual ICollection<EntryMark> EntryMarks { get; set; }
        public virtual ICollection<SubjectGroupDetail> SubjectGroupDetails { get; set; }
        public virtual ICollection<UserMajorDetail> UserMajorDetails { get; set; }
        public virtual ICollection<WeightNumber> WeightNumbers { get; set; }
    }
}
