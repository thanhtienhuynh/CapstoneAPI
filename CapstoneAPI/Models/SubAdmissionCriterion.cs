using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class SubAdmissionCriterion
    {
        public SubAdmissionCriterion()
        {
            EntryMarks = new HashSet<EntryMark>();
        }

        public int Id { get; set; }
        public int AdmissionCriterionId { get; set; }
        public int? Quantity { get; set; }
        public int? Gender { get; set; }
        public int? ProvinceId { get; set; }
        public int AdmissionMethodId { get; set; }
        public int Status { get; set; }

        public virtual AdmissionCriterion AdmissionCriterion { get; set; }
        public virtual AdmissionMethod AdmissionMethod { get; set; }
        public virtual Province Province { get; set; }
        public virtual ICollection<EntryMark> EntryMarks { get; set; }
    }
}
