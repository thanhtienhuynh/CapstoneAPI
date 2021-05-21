using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class SubjectGroupDetail
    {
        public SubjectGroupDetail()
        {
            SubjectWeights = new HashSet<SubjectWeight>();
        }

        public int Id { get; set; }
        public int SubjectGroupId { get; set; }
        public int? SubjectId { get; set; }
        public int? SpecialSubjectGroupId { get; set; }

        public virtual SpecialSubjectGroup SpecialSubjectGroup { get; set; }
        public virtual Subject Subject { get; set; }
        public virtual SubjectGroup SubjectGroup { get; set; }
        public virtual ICollection<SubjectWeight> SubjectWeights { get; set; }
    }
}
