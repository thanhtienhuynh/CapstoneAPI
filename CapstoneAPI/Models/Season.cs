using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class Season
    {
        public Season()
        {
            MajorDetails = new HashSet<MajorDetail>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime FromDate { get; set; }
        public int Status { get; set; }

        public virtual ICollection<MajorDetail> MajorDetails { get; set; }
    }
}
