using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class MajorDetail
    {
        public int Id { get; set; }
        public int UniversityId { get; set; }
        public int MajorId { get; set; }
        public string MajorCode { get; set; }
        public int TrainingProgramId { get; set; }
        public int SeasonId { get; set; }
        public int Status { get; set; }
        public DateTime UpdatedDate { get; set; }

        public virtual Major Major { get; set; }
        public virtual Season Season { get; set; }
        public virtual TrainingProgram TrainingProgram { get; set; }
        public virtual University University { get; set; }
        public virtual AdmissionCriterion AdmissionCriterion { get; set; }
    }
}
