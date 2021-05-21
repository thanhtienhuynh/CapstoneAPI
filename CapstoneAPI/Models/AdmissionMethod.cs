using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class AdmissionMethod
    {
        public AdmissionMethod()
        {
            SubAdmissionCriteria = new HashSet<SubAdmissionCriterion>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<SubAdmissionCriterion> SubAdmissionCriteria { get; set; }
    }
}
