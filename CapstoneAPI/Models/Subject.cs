using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class Subject
    {
        public Subject()
        {
            SubjectGroupDetails = new HashSet<SubjectGroupDetail>();
            Tests = new HashSet<Test>();
            Transcripts = new HashSet<Transcript>();
            WeightNumbers = new HashSet<WeightNumber>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }

        public virtual ICollection<SubjectGroupDetail> SubjectGroupDetails { get; set; }
        public virtual ICollection<Test> Tests { get; set; }
        public virtual ICollection<Transcript> Transcripts { get; set; }
        public virtual ICollection<WeightNumber> WeightNumbers { get; set; }
    }
}
