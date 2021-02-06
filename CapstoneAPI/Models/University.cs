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
            Tests = new HashSet<Test>();
            Tutions = new HashSet<Tution>();
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
        public virtual ICollection<Test> Tests { get; set; }
        public virtual ICollection<Tution> Tutions { get; set; }
    }
}
