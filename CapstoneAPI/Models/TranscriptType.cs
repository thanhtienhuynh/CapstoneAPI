using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class TranscriptType
    {
        public TranscriptType()
        {
            Ranks = new HashSet<Rank>();
            Transcripts = new HashSet<Transcript>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }

        public virtual ICollection<Rank> Ranks { get; set; }
        public virtual ICollection<Transcript> Transcripts { get; set; }
    }
}
