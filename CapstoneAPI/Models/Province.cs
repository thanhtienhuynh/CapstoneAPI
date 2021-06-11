using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class Province
    {
        public Province()
        {
            SubAdmissionCriteria = new HashSet<SubAdmissionCriterion>();
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int RegionId { get; set; }

        public virtual Region Region { get; set; }
        public virtual ICollection<SubAdmissionCriterion> SubAdmissionCriteria { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
