using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class AdmissionInformation
    {
        public AdmissionInformation()
        {
            UniversityAdmissionInformations = new HashSet<UniversityAdmissionInformation>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string PublishedPage { get; set; }
        public string RootUrl { get; set; }
        public DateTime? PostedDate { get; set; }
        public DateTime? CrawlerDate { get; set; }
        public int? ImportantLevel { get; set; }
        public int? Censor { get; set; }
        public int? Status { get; set; }

        public virtual ICollection<UniversityAdmissionInformation> UniversityAdmissionInformations { get; set; }
    }
}
