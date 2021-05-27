using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class AdmissionCriterion
    {
        public AdmissionCriterion()
        {
            SubAdmissionCriteria = new HashSet<SubAdmissionCriterion>();
        }

        public int MajorDetailId { get; set; }
        public int? Quantity { get; set; }

        public virtual MajorDetail MajorDetail { get; set; }
        public virtual ICollection<SubAdmissionCriterion> SubAdmissionCriteria { get; set; }
    }
}
