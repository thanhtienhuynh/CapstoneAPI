using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class TranscriptType
    {
        public TranscriptType()
        {
            Transcripts = new HashSet<Transcript>();
        }

        public int Id { get; set; }
        public string Type { get; set; }

        public virtual ICollection<Transcript> Transcripts { get; set; }
    }
}
