using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class SubjectGroup
    {
        public SubjectGroup()
        {
            SubjectGroupDetails = new HashSet<SubjectGroupDetail>();
            WeightNumbers = new HashSet<WeightNumber>();
        }

        public int Id { get; set; }
        public string GroupCode { get; set; }
        public int Status { get; set; }

        public virtual ICollection<SubjectGroupDetail> SubjectGroupDetails { get; set; }
        public virtual ICollection<WeightNumber> WeightNumbers { get; set; }
    }
}
