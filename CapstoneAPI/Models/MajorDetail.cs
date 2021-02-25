using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class MajorDetail
    {
        public int Id { get; set; }
        public int? UniversityId { get; set; }
        public int? MajorId { get; set; }
        public decimal Tuition { get; set; }
        public int? NumberOfStudents { get; set; }

        public virtual Major Major { get; set; }
        public virtual University University { get; set; }
    }
}
