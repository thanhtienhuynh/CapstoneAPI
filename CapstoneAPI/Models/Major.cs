using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class Major
    {
        public Major()
        {
            MajorCareers = new HashSet<MajorCareer>();
            Tutions = new HashSet<Tution>();
            WeightNumbers = new HashSet<WeightNumber>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int? Status { get; set; }

        public virtual ICollection<MajorCareer> MajorCareers { get; set; }
        public virtual ICollection<Tution> Tutions { get; set; }
        public virtual ICollection<WeightNumber> WeightNumbers { get; set; }
    }
}
