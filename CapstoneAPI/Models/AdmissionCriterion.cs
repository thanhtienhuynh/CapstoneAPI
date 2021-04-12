using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class AdmissionCriterion
    {
        public int Id { get; set; }
        public int? MajorDetailId { get; set; }
        public int? Year { get; set; }
        public int? Quantity { get; set; }

        public virtual MajorDetail MajorDetail { get; set; }
    }
}
