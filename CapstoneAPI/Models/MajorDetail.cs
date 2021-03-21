using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class MajorDetail
    {
        public MajorDetail()
        {
            EntryMarks = new HashSet<EntryMark>();
        }

        public int Id { get; set; }
        public int? UniversityId { get; set; }
        public int? MajorId { get; set; }
        public int? NumberOfStudents { get; set; }
        public string MajorCode { get; set; }

        public virtual Major Major { get; set; }
        public virtual University University { get; set; }
        public virtual ICollection<EntryMark> EntryMarks { get; set; }
    }
}
