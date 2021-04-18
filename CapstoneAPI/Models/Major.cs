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
            MajorDetails = new HashSet<MajorDetail>();
            UserMajors = new HashSet<UserMajor>();
            WeightNumbers = new HashSet<WeightNumber>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int? Status { get; set; }

        public virtual ICollection<MajorCareer> MajorCareers { get; set; }
        public virtual ICollection<MajorDetail> MajorDetails { get; set; }
        public virtual ICollection<UserMajor> UserMajors { get; set; }
        public virtual ICollection<WeightNumber> WeightNumbers { get; set; }
    }
}
