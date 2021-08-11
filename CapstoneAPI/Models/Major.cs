using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class Major
    {
        public Major()
        {
            MajorArticles = new HashSet<MajorArticle>();
            MajorCareers = new HashSet<MajorCareer>();
            MajorDetails = new HashSet<MajorDetail>();
            MajorSubjectGroups = new HashSet<MajorSubjectGroup>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Curriculum { get; set; }
        public string HumanQuality { get; set; }
        public string SalaryDescription { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int Status { get; set; }

        public virtual ICollection<MajorArticle> MajorArticles { get; set; }
        public virtual ICollection<MajorCareer> MajorCareers { get; set; }
        public virtual ICollection<MajorDetail> MajorDetails { get; set; }
        public virtual ICollection<MajorSubjectGroup> MajorSubjectGroups { get; set; }
    }
}
