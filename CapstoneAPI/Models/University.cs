using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class University
    {
        public University()
        {
            EntryMarks = new HashSet<EntryMark>();
            MajorDetails = new HashSet<MajorDetail>();
            Tests = new HashSet<Test>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string LogoUrl { get; set; }
        public string Infomation { get; set; }
        public int? Rating { get; set; }
        public int Status { get; set; }

        public virtual ICollection<EntryMark> EntryMarks { get; set; }
        public virtual ICollection<MajorDetail> MajorDetails { get; set; }
        public virtual ICollection<Test> Tests { get; set; }
    }
}
