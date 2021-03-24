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
            UniversityArticles = new HashSet<UniversityArticle>();
            UserUniversities = new HashSet<UserUniversity>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string LogoUrl { get; set; }
        public string Description { get; set; }
        public string Phone { get; set; }
        public string WebUrl { get; set; }
        public int? TuitionType { get; set; }
        public int? TuitionFrom { get; set; }
        public int? TuitionTo { get; set; }
        public int? Rating { get; set; }
        public int Status { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }

        public virtual ICollection<MajorDetail> MajorDetails { get; set; }
        public virtual ICollection<Test> Tests { get; set; }
        public virtual ICollection<UniversityArticle> UniversityArticles { get; set; }
        public virtual ICollection<UserUniversity> UserUniversities { get; set; }
    }
}
