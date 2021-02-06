using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class Transcript
    {
        public int Id { get; set; }
        public double Grade { get; set; }
        public DateTime DateRecord { get; set; }
        public int TranscriptTypeId { get; set; }
        public int UserId { get; set; }
        public int? SubjectId { get; set; }

        public virtual Subject Subject { get; set; }
        public virtual TranscriptType TranscriptType { get; set; }
        public virtual User User { get; set; }
    }
}
