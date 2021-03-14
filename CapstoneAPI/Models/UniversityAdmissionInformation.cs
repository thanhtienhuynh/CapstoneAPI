using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class UniversityAdmissionInformation
    {
        public int UniversityId { get; set; }
        public int AdmissionInformationId { get; set; }

        public virtual AdmissionInformation AdmissionInformation { get; set; }
        public virtual University University { get; set; }
    }
}
