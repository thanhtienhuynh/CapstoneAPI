using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class SpecialSubjectGroup
    {
        public SpecialSubjectGroup()
        {
            SubjectGroupDetails = new HashSet<SubjectGroupDetail>();
            Subjects = new HashSet<Subject>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public virtual ICollection<SubjectGroupDetail> SubjectGroupDetails { get; set; }
        public virtual ICollection<Subject> Subjects { get; set; }
    }
}
