using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class MajorDetail
    {
        public MajorDetail()
        {
            AdmissionCriteria = new HashSet<AdmissionCriterion>();
            EntryMarks = new HashSet<EntryMark>();
            UserMajorDetails = new HashSet<UserMajorDetail>();
        }

        public int Id { get; set; }
        public int? UniversityId { get; set; }
        public int? MajorId { get; set; }
        public string MajorCode { get; set; }
        public int TrainingProgramId { get; set; }

        public virtual Major Major { get; set; }
        public virtual TrainingProgram TrainingProgram { get; set; }
        public virtual University University { get; set; }
        public virtual ICollection<AdmissionCriterion> AdmissionCriteria { get; set; }
        public virtual ICollection<EntryMark> EntryMarks { get; set; }
        public virtual ICollection<UserMajorDetail> UserMajorDetails { get; set; }
    }
}
