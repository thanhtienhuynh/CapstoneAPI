using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class University
    {
        public University()
        {
            MajorDetails = new HashSet<MajorDetail>();
            Tests = new HashSet<Test>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string LogoUrl { get; set; }
        public string Description { get; set; }
        public int? Rating { get; set; }
        public int Status { get; set; }
        public string Tuition { get; set; }
        public string Phone { get; set; }
        public string WebUrl { get; set; }

        public virtual ICollection<MajorDetail> MajorDetails { get; set; }
        public virtual ICollection<Test> Tests { get; set; }
    }
}
